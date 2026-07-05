<?php
/**
 * POST /staff/create.php
 *
 * Creates a new staff account (admin only) with a randomly generated
 * 12-character temporary password. Sets must_change_password = 1. The
 * plain temporary password is returned once in the response — it is
 * never stored and cannot be recovered.
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

$body      = json_decode(file_get_contents('php://input'), true);
$username  = trim(strip_tags($body['username']   ?? ''));
$email     = trim(strip_tags($body['email']      ?? ''));
$firstName = trim(strip_tags($body['first_name'] ?? ''));
$lastName  = trim(strip_tags($body['last_name']  ?? ''));
$role      = in_array($body['role'] ?? '', ['admin', 'staff'], true) ? $body['role'] : 'staff';

if (!$username || !$firstName || !$lastName) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Username, first name, and last name are required']);
    exit();
}
$email = $email ?: null;

$tempPass = generateTempPassword(12);
$hash     = password_hash($tempPass, PASSWORD_BCRYPT, ['cost' => 12]);

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'INSERT INTO staff (username, email, password_hash, first_name, last_name, role, must_change_password)
         VALUES (?, ?, ?, ?, ?, ?, 1)'
    );
    $stmt->execute([$username, $email, $hash, $firstName, $lastName, $role]);

    http_response_code(201);
    echo json_encode([
        'success' => true,
        'data'    => ['id' => (int)$db->lastInsertId(), 'temp_password' => $tempPass],
    ]);
} catch (\Throwable $e) {
    if ((int)$e->getCode() === 23000 || str_contains($e->getMessage(), '23000')) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Username already exists']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
