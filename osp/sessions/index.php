<?php
/**
 * GET /sessions/index.php?project_id=X
 *
 * Returns all sessions for a project from the session_summary view,
 * ordered by session_number ascending.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$projectId = (int)($_GET['project_id'] ?? 0);

try {
    $db   = getDb();
    $stmt = $db->prepare('SELECT * FROM session_summary WHERE project_id=? ORDER BY session_number');
    $stmt->execute([$projectId]);
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
