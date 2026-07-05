<?php
/**
 * POST /auth/login.php
 *
 * Authenticates a staff member by username and password and returns a
 * signed JWT access token (8 hours) plus the staff record. If
 * must_change_password is 1 on the returned staff object, the client
 * MUST force a password change before allowing access to anything else.
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

$body     = json_decode(file_get_contents('php://input'), true);
$username = trim(strip_tags($body['username'] ?? ''));
$password = $body['password'] ?? '';

if (!$username || !$password) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Username and password are required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'SELECT id, username, email, first_name, last_name, role,
                password_hash, must_change_password, is_active
         FROM staff WHERE username = ?'
    );
    $stmt->execute([$username]);
    $staff = $stmt->fetch();

    if (!$staff || !$staff['is_active'] || !password_verify($password, $staff['password_hash'])) {
        http_response_code(401);
        echo json_encode(['success' => false, 'error' => 'Invalid username or password']);
        exit();
    }

    $upd = $db->prepare('UPDATE staff SET last_login = NOW() WHERE id = ?');
    $upd->execute([$staff['id']]);

    $token = generateAccessToken((int)$staff['id'], $staff['role'], (int)$staff['must_change_password']);

    echo json_encode([
        'success' => true,
        'data'    => [
            'token' => $token,
            'staff' => [
                'id'                    => (int)$staff['id'],
                'username'              => $staff['username'],
                'email'                 => $staff['email'],
                'first_name'            => $staff['first_name'],
                'last_name'             => $staff['last_name'],
                'role'                  => $staff['role'],
                'must_change_password' => (int)$staff['must_change_password'],
            ],
        ],
    ]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
