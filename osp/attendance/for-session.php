<?php
/**
 * GET /attendance/for-session.php?session_id=X
 *
 * Returns all enrolled students for a session's project, each with their
 * existing attendance record for this session (0 if not yet entered).
 * Also returns session metadata and the available_minutes for the
 * session. Used to pre-populate the attendance entry form.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$sessionId = (int)($_GET['session_id'] ?? 0);

try {
    $db = getDb();

    $sessStmt = $db->prepare('SELECT project_id, start_time, end_time FROM sessions WHERE id=?');
    $sessStmt->execute([$sessionId]);
    $sess = $sessStmt->fetch();
    if (!$sess) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Session not found']);
        exit();
    }

    $projectId = $sess['project_id'];
    $availMins = (strtotime($sess['end_time']) - strtotime($sess['start_time'])) / 60;

    $stmt = $db->prepare(
        'SELECT ps.id AS project_student_id,
                s.id AS student_id,
                s.candidate_number,
                s.cis_ref,
                s.surname,
                s.first_name,
                sps.time_extension_percent,
                sps.rest_breaks,
                sps.total_minutes_allowed,
                sps.total_minutes_used,
                sps.minutes_remaining,
                COALESCE(sa.minutes_present, 0) AS minutes_present,
                sa.id AS attendance_id
         FROM project_students ps
         JOIN students s ON s.id = ps.student_id
         JOIN student_project_summary sps ON sps.project_student_id = ps.id
         LEFT JOIN session_attendance sa ON sa.session_id=? AND sa.project_student_id=ps.id
         WHERE ps.project_id=? AND s.is_active=1
         ORDER BY s.surname, s.first_name'
    );
    $stmt->execute([$sessionId, $projectId]);

    echo json_encode([
        'success' => true,
        'data'    => [
            'session'            => $sess,
            'available_minutes'  => $availMins,
            'students'           => $stmt->fetchAll(),
        ],
    ]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
