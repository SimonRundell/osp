<?php
/**
 * POST /auth/change-password.php
 *
 * Changes the authenticated staff member's password. Password rules,
 * enforced independently of any client-side validation:
 *   - Minimum 8 characters
 *   - At least one uppercase ASCII letter [A-Z]
 *   - At least one ASCII digit [0-9]
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../config.php';
require_once __DIR__ . '/../jwt.php';

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$decoded = requireAuth();
$staffId = (int)$decoded->sub;

$body            = json_decode(file_get_contents('php://input'), true);
$currentPassword = $body['current_password'] ?? '';
$newPassword     = $body['new_password']     ?? '';

if (!$currentPassword || !$newPassword) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Both current and new password are required']);
    exit();
}
if (strlen($newPassword) < 8 || !preg_match('/[A-Z]/', $newPassword) || !preg_match('/[0-9]/', $newPassword)) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'New password must be at least 8 characters with one uppercase letter and one digit']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare('SELECT password_hash FROM staff WHERE id = ?');
    $stmt->execute([$staffId]);
    $staff = $stmt->fetch();

    if (!$staff || !password_verify($currentPassword, $staff['password_hash'])) {
        http_response_code(401);
        echo json_encode(['success' => false, 'error' => 'Current password is incorrect']);
        exit();
    }

    $hash = password_hash($newPassword, PASSWORD_BCRYPT, ['cost' => 12]);
    $upd  = $db->prepare('UPDATE staff SET password_hash = ?, must_change_password = 0 WHERE id = ?');
    $upd->execute([$hash, $staffId]);

    echo json_encode(['success' => true, 'data' => ['message' => 'Password changed successfully']]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
