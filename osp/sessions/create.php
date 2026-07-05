<?php
/**
 * POST /sessions/create.php
 *
 * Creates a new session for a project. Session number is assigned
 * atomically inside a transaction using a FOR UPDATE lock to prevent
 * duplicate numbers under concurrent requests. For session_type =
 * 'individual', a student_project_id must be supplied and a zero-minute
 * attendance record is pre-created so the student appears on the
 * attendance entry form.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body             = json_decode(file_get_contents('php://input'), true);
$projectId        = (int)($body['project_id']        ?? 0);
$sessionDate      = trim($body['session_date']        ?? '');
$startTime        = trim($body['start_time']          ?? '');
$endTime          = trim($body['end_time']            ?? '');
$supervisorId     = (int)($body['supervisor_id']      ?? 0);
$sessionType      = in_array($body['session_type'] ?? '', ['class', 'individual'], true) ? $body['session_type'] : 'class';
$notes            = trim(strip_tags($body['notes']    ?? '')) ?: null;
$studentProjectId = (int)($body['student_project_id'] ?? 0);

if (!$projectId || !$sessionDate || !$startTime || !$endTime || !$supervisorId) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'project_id, session_date, start_time, end_time and supervisor_id are required']);
    exit();
}
if ($startTime >= $endTime) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Start time must be before end time']);
    exit();
}

$db = getDb();
$db->beginTransaction();
try {
    $numStmt = $db->prepare('SELECT COALESCE(MAX(session_number),0)+1 AS next_num FROM sessions WHERE project_id=? FOR UPDATE');
    $numStmt->execute([$projectId]);
    $sessionNumber = (int)$numStmt->fetch()['next_num'];

    $ins = $db->prepare(
        'INSERT INTO sessions (project_id, session_number, session_date, start_time, end_time, supervisor_id, session_type, notes)
         VALUES (?,?,?,?,?,?,?,?)'
    );
    $ins->execute([$projectId, $sessionNumber, $sessionDate, $startTime, $endTime, $supervisorId, $sessionType, $notes]);
    $sessionId = (int)$db->lastInsertId();

    if ($sessionType === 'individual' && $studentProjectId) {
        $attIns = $db->prepare(
            'INSERT INTO session_attendance (session_id, project_student_id, minutes_present) VALUES (?,?,0)'
        );
        $attIns->execute([$sessionId, $studentProjectId]);
    }

    $db->commit();
    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => $sessionId, 'session_number' => $sessionNumber]]);
} catch (\Throwable $e) {
    $db->rollBack();
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Could not create session']);
}
