<?php
/**
 * GET /students/index.php
 *
 * Returns all active students ordered by surname, first_name.
 * Deactivated students are excluded.
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

try {
    $db   = getDb();
    $stmt = $db->query(
        'SELECT id, candidate_number, cis_ref, surname, first_name, is_active
         FROM students WHERE is_active=1 ORDER BY surname, first_name'
    );
    echo json_encode(['success' => true, 'data' => $stmt->fetchAll()]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
