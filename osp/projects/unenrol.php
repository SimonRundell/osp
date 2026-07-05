<?php
/**
 * DELETE /projects/unenrol.php
 *
 * Removes a student from a project (admin only). Because
 * session_attendance records CASCADE delete, all attendance history for
 * this student on this project is permanently deleted. If the student
 * has any minutes_present > 0, a 409 conflict is returned on the first
 * call; the admin must re-send with confirm: true to proceed.
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
$psId    = (int)($body['project_student_id'] ?? 0);
$confirm = (bool)($body['confirm']           ?? false);

try {
    $db  = getDb();
    $chk = $db->prepare(
        'SELECT COUNT(*) FROM session_attendance WHERE project_student_id=? AND minutes_present > 0'
    );
    $chk->execute([$psId]);
    $cnt = (int)$chk->fetchColumn();

    if ($cnt > 0 && !$confirm) {
        http_response_code(409);
        echo json_encode([
            'success' => false,
            'error'   => "This student has $cnt attendance records. Send confirm:true to unenrol anyway.",
            'conflict'=> true,
        ]);
        exit();
    }

    $stmt = $db->prepare('DELETE FROM project_students WHERE id=?');
    $stmt->execute([$psId]);
    echo json_encode(['success' => true, 'data' => ['message' => 'Student unenrolled']]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
