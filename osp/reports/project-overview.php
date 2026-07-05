<?php
/**
 * GET /reports/project-overview.php?project_id=X
 *
 * Returns the complete data set needed to render the project report,
 * the print view, and to generate CSV/Excel exports. A single call
 * fetches all sessions, all enrolled students with their running
 * totals, and per-student per-session attendance.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

$projectId = (int)($_GET['project_id'] ?? 0);

try {
    $db = getDb();

    $pStmt = $db->prepare(
        "SELECT p.*, CONCAT(s.first_name,' ',s.last_name) AS creator_name
         FROM projects p LEFT JOIN staff s ON s.id=p.created_by WHERE p.id=?"
    );
    $pStmt->execute([$projectId]);
    $project = $pStmt->fetch();
    if (!$project) {
        http_response_code(404);
        echo json_encode(['success' => false, 'error' => 'Project not found']);
        exit();
    }

    $sStmt = $db->prepare('SELECT * FROM session_summary WHERE project_id=? ORDER BY session_number');
    $sStmt->execute([$projectId]);
    $sessions = $sStmt->fetchAll();
    $totalAvailableMinutes = 0;
    foreach ($sessions as $row) {
        $totalAvailableMinutes += (float)$row['available_minutes'];
    }

    $stStmt = $db->prepare('SELECT sps.* FROM student_project_summary sps WHERE sps.project_id=? ORDER BY sps.surname, sps.first_name');
    $stStmt->execute([$projectId]);
    $students = $stStmt->fetchAll();

    $attStmt = $db->prepare(
        'SELECT sa.session_id, se.session_number, sa.minutes_present
         FROM session_attendance sa
         JOIN sessions se ON se.id=sa.session_id
         WHERE sa.project_student_id=?
         ORDER BY se.session_number'
    );
    foreach ($students as &$student) {
        $attStmt->execute([$student['project_student_id']]);
        $student['attendance'] = $attStmt->fetchAll();
    }
    unset($student);

    echo json_encode([
        'success' => true,
        'data'    => [
            'project'                 => $project,
            'sessions'                => $sessions,
            'students'                => $students,
            'total_available_minutes' => $totalAvailableMinutes,
            'generated_at'            => date('Y-m-d H:i:s'),
        ],
    ]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
