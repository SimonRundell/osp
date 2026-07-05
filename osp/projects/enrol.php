<?php
/**
 * POST /projects/enrol.php
 *
 * Enrols a student onto a project (admin only) by creating a
 * project_students record with their individual access arrangements. A
 * student may only be enrolled once per project (enforced by unique key).
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

$body                 = json_decode(file_get_contents('php://input'), true);
$projectId            = (int)($body['project_id']             ?? 0);
$studentId            = (int)($body['student_id']             ?? 0);
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
        'INSERT INTO project_students (project_id, student_id, time_extension_percent, rest_breaks, notes)
         VALUES (?,?,?,?,?)'
    );
    $stmt->execute([$projectId, $studentId, $timeExtensionPercent, $restBreaks, $notes]);

    http_response_code(201);
    echo json_encode(['success' => true, 'data' => ['id' => (int)$db->lastInsertId()]]);
} catch (\Throwable $e) {
    if ((int)$e->getCode() === 23000 || str_contains($e->getMessage(), '23000')) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Student is already enrolled on this project']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Could not enrol student']);
    }
}
