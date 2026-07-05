<?php
/**
 * GET /sessions/next-number.php?project_id=X
 *
 * Returns the next available sequential session number for a project
 * (MAX(session_number) + 1, or 1 if no sessions exist). Used to preview
 * the number before creating a session.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$projectId = (int)($_GET['project_id'] ?? 0);

try {
    $db   = getDb();
    $stmt = $db->prepare('SELECT COALESCE(MAX(session_number),0)+1 AS next_num FROM sessions WHERE project_id=?');
    $stmt->execute([$projectId]);
    $row = $stmt->fetch();
    echo json_encode(['success' => true, 'data' => ['next_session_number' => (int)$row['next_num']]]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
