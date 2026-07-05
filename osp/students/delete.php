<?php
/**
 * DELETE /students/delete.php
 *
 * Soft-deactivates a student (sets is_active = 0), admin only. The
 * student record and all historical attendance data are preserved.
 * Deactivated students no longer appear in enrolment or attendance lists.
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
    $db   = getDb();
    $stmt = $db->prepare('UPDATE students SET is_active=0 WHERE id=?');
    $stmt->execute([$id]);
    echo json_encode(['success' => true, 'data' => ['message' => 'Student deactivated']]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
