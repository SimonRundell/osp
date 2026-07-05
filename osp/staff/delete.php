<?php
/**
 * DELETE /staff/delete.php
 *
 * Soft-deactivates a staff member (sets is_active = 0), admin only. The
 * record and all associated data are preserved. Blocked if the target is
 * the only remaining active admin, to prevent accidental lockout.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

$decoded = requireAuth();
requireAdmin($decoded);

if ($_SERVER['REQUEST_METHOD'] !== 'DELETE') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body = json_decode(file_get_contents('php://input'), true);
$id   = (int)($body['id'] ?? 0);

try {
    $db = getDb();

    $cnt = (int)$db->query("SELECT COUNT(*) FROM staff WHERE role='admin' AND is_active=1")->fetchColumn();

    $self = $db->prepare('SELECT role FROM staff WHERE id=?');
    $self->execute([$id]);
    $target = $self->fetch();

    if ($target && $target['role'] === 'admin' && $cnt <= 1) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Cannot deactivate the only admin account']);
        exit();
    }

    $stmt = $db->prepare('UPDATE staff SET is_active=0 WHERE id=?');
    $stmt->execute([$id]);
    echo json_encode(['success' => true, 'data' => ['message' => 'Staff member deactivated']]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
