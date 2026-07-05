<?php
/**
 * PUT /projects/update-enrolment.php
 *
 * Updates the access arrangement flags for an enrolled student (admin
 * only). Changing time_extension_percent retroactively alters the
 * student's total_minutes_allowed (calculated, never stored).
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

$body                 = json_decode(file_get_contents('php://input'), true);
$psId                 = (int)($body['project_student_id']     ?? 0);
$timeExtensionPercent = (int)($body['time_extension_percent'] ?? 0);
$restBreaks           = (int)(bool)($body['rest_breaks']      ?? 0);
$notes                = trim(strip_tags($body['notes']        ?? '')) ?: null;

if (!in_array($timeExtensionPercent, [0, 10, 20, 25], true)) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'time_extension_percent must be 0, 10, 20, or 25']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'UPDATE project_students SET time_extension_percent=?, rest_breaks=?, notes=? WHERE id=?'
    );
    $stmt->execute([$timeExtensionPercent, $restBreaks, $notes, $psId]);
    echo json_encode(['success' => true, 'data' => ['message' => 'Enrolment updated']]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
