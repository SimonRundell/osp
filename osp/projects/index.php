<?php
/**
 * GET /projects/index.php
 *
 * Returns all projects (active and inactive), ordered by year descending
 * then name. Includes the creator's full name, enrolled student count,
 * and total scheduled minutes (used by the dashboard to display
 * remaining unscheduled time: base_hours×60 − scheduled_minutes).
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

try {
    $db   = getDb();
    $stmt = $db->query(
        "SELECT p.id, p.name, p.description, p.year, p.centre_number,
                p.base_hours, p.start_date, p.end_date, p.created_by,
                p.is_active, p.created_at,
                CONCAT(s.first_name,' ',s.last_name) AS creator_name,
                (SELECT COUNT(*) FROM project_students ps WHERE ps.project_id = p.id) AS student_count,
                (SELECT COALESCE(SUM(TIMESTAMPDIFF(MINUTE, se.start_time, se.end_time)), 0)
                 FROM sessions se WHERE se.project_id = p.id) AS scheduled_minutes
         FROM projects p
         LEFT JOIN staff s ON s.id = p.created_by
         ORDER BY p.year DESC, p.name"
    );
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
