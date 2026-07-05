<?php
/**
 * DELETE /sessions/delete.php
 *
 * Deletes a session and all its attendance records (CASCADE), admin
 * only. If any attendance records have minutes_present > 0, a 409
 * conflict is returned on the first call. Re-send with confirm: true to
 * force deletion.
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

$body    = json_decode(file_get_contents('php://input'), true);
$id      = (int)($body['id']      ?? 0);
$confirm = (bool)($body['confirm']?? false);

try {
    $db  = getDb();
    $chk = $db->prepare('SELECT COUNT(*) FROM session_attendance WHERE session_id=? AND minutes_present>0');
    $chk->execute([$id]);
    $cnt = (int)$chk->fetchColumn();

    if ($cnt > 0 && !$confirm) {
        http_response_code(409);
        echo json_encode([
            'success' => false,
            'error'   => "Session has $cnt attendance records. Send confirm:true to delete anyway.",
            'conflict'=> true,
        ]);
        exit();
    }

    $del = $db->prepare('DELETE FROM sessions WHERE id=?');
    $del->execute([$id]);
    echo json_encode(['success' => true, 'data' => ['message' => 'Session deleted']]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
