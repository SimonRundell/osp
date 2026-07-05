<?php
/**
 * POST /attendance/save.php
 *
 * Upserts attendance records for all students in a session using
 * INSERT … ON DUPLICATE KEY UPDATE in a single transaction. Negative
 * minutes_present values are silently clamped to 0. Safe to call
 * repeatedly — the session_attendance table has a unique key on
 * (session_id, project_student_id), so later calls overwrite earlier
 * values.
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

$body       = json_decode(file_get_contents('php://input'), true);
$sessionId  = (int)($body['session_id']  ?? 0);
$attendance = $body['attendance']         ?? [];

if (!$sessionId || !is_array($attendance)) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'session_id and attendance array are required']);
    exit();
}

$db = getDb();

$sessStmt = $db->prepare('SELECT id FROM sessions WHERE id=?');
$sessStmt->execute([$sessionId]);
if (!$sessStmt->fetch()) {
    http_response_code(404);
    echo json_encode(['success' => false, 'error' => 'Session not found']);
    exit();
}

$db->beginTransaction();
try {
    $upsert = $db->prepare(
        'INSERT INTO session_attendance (session_id, project_student_id, minutes_present)
         VALUES (?,?,?)
         ON DUPLICATE KEY UPDATE minutes_present=VALUES(minutes_present)'
    );
    $saved = 0;
    foreach ($attendance as $rec) {
        $psId    = (int)($rec['project_student_id'] ?? 0);
        $minutes = (int)($rec['minutes_present']    ?? 0);
        if ($minutes < 0) $minutes = 0;
        $upsert->execute([$sessionId, $psId, $minutes]);
        $saved++;
    }
    $db->commit();
    echo json_encode(['success' => true, 'data' => ['message' => "Attendance saved ($saved records)", 'saved' => $saved]]);
} catch (\Throwable $e) {
    $db->rollBack();
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Could not save attendance']);
}
