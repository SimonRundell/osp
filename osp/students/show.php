<?php
/**
 * GET /students/show.php?id=X
 *
 * Returns a single student record by primary key.
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
        'SELECT id, candidate_number, cis_ref, surname, first_name, is_active
         FROM students WHERE id=?'
    );
    $stmt->execute([$id]);
    $row = $stmt->fetch();

    if (!$row) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Student not found']);
        exit();
    }
    echo json_encode(['success' => true, 'data' => $row]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
