<?php
/**
 * POST /staff/reset-password.php
 *
 * Generates a new random 12-character temporary password for the
 * specified staff member (admin only), hashes it, and sets
 * must_change_password = 1. The plain temporary password is returned
 * once for the admin to communicate securely to the staff member.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

$decoded = requireAuth();
requireAdmin($decoded);

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body = json_decode(file_get_contents('php://input'), true);
$id   = (int)($body['id'] ?? 0);

$tempPass = generateTempPassword(12);
$hash     = password_hash($tempPass, PASSWORD_BCRYPT, ['cost' => 12]);

try {
    $db   = getDb();
    $stmt = $db->prepare('UPDATE staff SET password_hash=?, must_change_password=1 WHERE id=?');
    $stmt->execute([$hash, $id]);
    echo json_encode(['success' => true, 'data' => ['temp_password' => $tempPass]]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
