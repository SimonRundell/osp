<?php
/**
 * GET /reports/all-projects-summary.php
 *
 * Returns summary statistics for all active projects (admin only).
 * Aggregates student counts and total minutes used across each project.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

$decoded = requireAuth();
requireAdmin($decoded);

try {
    $db   = getDb();
    $stmt = $db->query(
        'SELECT p.id, p.name, p.year, p.base_hours, p.is_active,
                COUNT(DISTINCT ps.id) AS student_count,
                COALESCE(SUM(sa.minutes_present),0) AS total_minutes_used,
                ROUND(p.base_hours * 60 * COUNT(DISTINCT ps.id)) AS total_minutes_allowed
         FROM projects p
         LEFT JOIN project_students ps ON ps.project_id=p.id
         LEFT JOIN session_attendance sa ON sa.project_student_id=ps.id
         WHERE p.is_active=1
         GROUP BY p.id
         ORDER BY p.year DESC, p.name'
    );
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
