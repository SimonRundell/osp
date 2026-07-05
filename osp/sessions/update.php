<?php
/**
 * PUT /sessions/update.php
 *
 * Updates the mutable fields of an existing session (admin only).
 * session_type cannot be changed after creation; session number is
 * immutable.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

$decoded = requireAuth();
requireAdmin($decoded);

if ($_SERVER['REQUEST_METHOD'] !== 'PUT') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body         = json_decode(file_get_contents('php://input'), true);
$id           = (int)($body['id']            ?? 0);
$sessionDate  = trim($body['session_date']    ?? '');
$startTime    = trim($body['start_time']      ?? '');
$endTime      = trim($body['end_time']        ?? '');
$supervisorId = (int)($body['supervisor_id'] ?? 0);
$notes        = trim(strip_tags($body['notes'] ?? '')) ?: null;

if ($startTime >= $endTime) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Start time must be before end time']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'UPDATE sessions SET session_date=?, start_time=?, end_time=?, supervisor_id=?, notes=? WHERE id=?'
    );
    $stmt->execute([$sessionDate, $startTime, $endTime, $supervisorId, $notes, $id]);
    echo json_encode(['success' => true, 'data' => ['message' => 'Session updated']]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
