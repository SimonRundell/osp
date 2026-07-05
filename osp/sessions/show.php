<?php
/**
 * GET /sessions/show.php?id=X
 *
 * Returns a single session plus all of its attendance records (joined
 * with student name and candidate number).
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$id = (int)($_GET['id'] ?? 0);

try {
    $db   = getDb();
    $stmt = $db->prepare('SELECT * FROM session_summary WHERE session_id=?');
    $stmt->execute([$id]);
    $session = $stmt->fetch();

    if (!$session) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Session not found']);
        exit();
    }

    $att = $db->prepare(
        'SELECT sa.id, sa.project_student_id, sa.minutes_present,
                s.candidate_number, s.surname, s.first_name
         FROM session_attendance sa
         JOIN project_students ps ON ps.id = sa.project_student_id
         JOIN students s ON s.id = ps.student_id
         WHERE sa.session_id=?'
    );
    $att->execute([$id]);
    $session['attendance'] = $att->fetchAll();

    echo json_encode(['success' => true, 'data' => $session]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
