<?php
/**
 * GET /students/for-project.php?project_id=X
 *
 * Returns all students enrolled on a given project, joined with their
 * running time totals from the student_project_summary view. Used to
 * populate attendance forms and the project detail page.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$projectId = (int)($_GET['project_id'] ?? 0);

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'SELECT sps.project_student_id, sps.student_id, sps.candidate_number,
                sps.cis_ref, sps.surname, sps.first_name,
                sps.time_extension_percent, sps.rest_breaks, sps.notes,
                sps.total_minutes_allowed, sps.total_minutes_used, sps.minutes_remaining
         FROM student_project_summary sps
         WHERE sps.project_id = ?
         ORDER BY sps.surname, sps.first_name'
    );
    $stmt->execute([$projectId]);
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
