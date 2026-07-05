<?php
/**
 * GET /staff/index.php
 *
 * Returns all staff members ordered by last_name, first_name. Inactive
 * accounts are included so admins can see the full list.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

try {
    $db   = getDb();
    $stmt = $db->query(
        'SELECT id, username, email, first_name, last_name, role,
                is_active, must_change_password, last_login, created_at
         FROM staff ORDER BY last_name, first_name'
    );
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
