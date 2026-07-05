<?php
/**
 * GET /projects/show.php?id=X
 *
 * Returns a single project by primary key, including aggregate counts of
 * enrolled students and completed sessions.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$id = (int)($_GET['id'] ?? 0);

try {
    $db   = getDb();
    $stmt = $db->prepare(
        "SELECT p.*, CONCAT(s.first_name,' ',s.last_name) AS creator_name,
                (SELECT COUNT(*) FROM project_students ps WHERE ps.project_id=p.id) AS student_count,
                (SELECT COUNT(*) FROM sessions se WHERE se.project_id=p.id) AS session_count
         FROM projects p
         LEFT JOIN staff s ON s.id = p.created_by
         WHERE p.id=?"
    );
    $stmt->execute([$id]);
    $row = $stmt->fetch();

    if (!$row) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Project not found']);
        exit();
    }
    echo json_encode(['success' => true, 'data' => $row]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
