<?php
/**
 * JWT utility functions for the OSP Hours Tracker.
 *
 * Provides token generation and request authentication using the
 * firebase/php-jwt library. A single access token is issued at login
 * (8 hours by default) — there is no refresh token; when the token
 * expires the desktop client returns the user to the login screen.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/vendor/autoload.php';
require_once __DIR__ . '/config.php';

use Firebase\JWT\JWT;
use Firebase\JWT\Key;

/**
 * Generates a signed JWT access token for a staff member.
 *
 * @param int    $staffId             The authenticated staff member's database ID.
 * @param string $role                'admin' or 'staff'.
 * @param int    $mustChangePassword  1 if the account must change its password before use, else 0.
 * @return string Signed JWT string.
 */
function generateAccessToken(int $staffId, string $role, int $mustChangePassword): string {
    $jwt = getJwtConfig();
    $payload = [
        'iss'                  => 'osptracker',
        'iat'                  => time(),
        'exp'                  => time() + $jwt['accessExpiry'],
        'sub'                  => $staffId,
        'role'                 => $role,
        'must_change_password' => $mustChangePassword,
    ];
    return JWT::encode($payload, $jwt['secret'], 'HS256');
}

/**
 * Validates the Bearer token in the Authorization header.
 *
 * Exits with HTTP 401 and a JSON error body if no token is
 * present or if the token is invalid/expired.
 *
 * @return object Decoded JWT payload (stdClass) with sub, role, must_change_password.
 */
function requireAuth(): object {
    $jwt     = getJwtConfig();
    $headers = getallheaders();
    $auth    = $headers['Authorization']
            ?? $headers['authorization']
            ?? $_SERVER['HTTP_AUTHORIZATION']
            ?? $_SERVER['REDIRECT_HTTP_AUTHORIZATION']
            ?? '';

    if (!preg_match('/^Bearer\s+(.+)$/', $auth, $matches)) {
        http_response_code(401);
        echo json_encode(['success' => false, 'error' => 'No token provided']);
        exit();
    }

    try {
        return JWT::decode($matches[1], new Key($jwt['secret'], 'HS256'));
    } catch (Exception $e) {
        http_response_code(401);
        echo json_encode(['success' => false, 'error' => 'Invalid or expired token']);
        exit();
    }
}

/**
 * Requires the decoded JWT to carry the 'admin' role.
 *
 * Exits with HTTP 403 and a JSON error body if the caller is not an admin.
 *
 * @param object $decoded Decoded JWT payload, as returned by requireAuth().
 * @return void
 */
function requireAdmin(object $decoded): void {
    if (($decoded->role ?? '') !== 'admin') {
        http_response_code(403);
        echo json_encode(['success' => false, 'error' => 'Admin access required']);
        exit();
    }
}

/**
 * Generates a random alphanumeric temporary password.
 *
 * @param int $length Desired password length.
 * @return string Random password.
 */
function generateTempPassword(int $length = 12): string {
    $chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789@#!';
    $pass  = '';
    for ($i = 0; $i < $length; $i++) {
        $pass .= $chars[random_int(0, strlen($chars) - 1)];
    }
    return $pass;
}
