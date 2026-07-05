<?php
/**
 * PUT /staff/update.php
 *
 * Updates a staff member's editable fields (admin only). Username cannot
 * be changed after creation; passwords are managed via reset-password.php.
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

$body      = json_decode(file_get_contents('php://input'), true);
$id        = (int)($body['id']         ?? 0);
$email     = trim(strip_tags($body['email']      ?? ''));
$firstName = trim(strip_tags($body['first_name'] ?? ''));
$lastName  = trim(strip_tags($body['last_name']  ?? ''));
$role      = in_array($body['role'] ?? '', ['admin', 'staff'], true) ? $body['role'] : 'staff';
$isActive  = isset($body['is_active']) ? (int)(bool)$body['is_active'] : 1;

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'UPDATE staff SET email=?, first_name=?, last_name=?, role=?, is_active=? WHERE id=?'
    );
    $stmt->execute([$email ?: null, $firstName, $lastName, $role, $isActive, $id]);
    echo json_encode(['success' => true, 'data' => ['message' => 'Staff member updated']]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
