<?php
/**
 * GET /attendance/student-summary.php?project_student_id=X
 *
 * Returns a session-by-session attendance breakdown for a single
 * enrolled student, plus their overall running totals from the
 * student_project_summary view.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$psId = (int)($_GET['project_student_id'] ?? 0);

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'SELECT sa.id, sa.session_id, se.session_number, se.session_date,
                se.start_time, se.end_time,
                TIME_TO_SEC(TIMEDIFF(se.end_time, se.start_time))/60 AS available_minutes,
                sa.minutes_present
         FROM session_attendance sa
         JOIN sessions se ON se.id = sa.session_id
         WHERE sa.project_student_id=?
         ORDER BY se.session_number'
    );
    $stmt->execute([$psId]);
    $records = $stmt->fetchAll();

    $tot = $db->prepare(
        'SELECT total_minutes_allowed, total_minutes_used, minutes_remaining
         FROM student_project_summary WHERE project_student_id=?'
    );
    $tot->execute([$psId]);
    $totals = $tot->fetch();

    echo json_encode(['success' => true, 'data' => ['sessions' => $records, 'totals' => $totals]]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
