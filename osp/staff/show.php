<?php
/**
 * GET /staff/show.php?id=X
 *
 * Returns a single staff member by primary key.
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
        'SELECT id, username, email, first_name, last_name, role,
                is_active, must_change_password, last_login, created_at
         FROM staff WHERE id = ?'
    );
    $stmt->execute([$id]);
    $row = $stmt->fetch();

    if (!$row) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Staff member not found']);
        exit();
    }
    echo json_encode(['success' => true, 'data' => $row]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
