<?php
/**
 * PUT /projects/update.php
 *
 * Updates all editable fields on a project, including toggling is_active
 * (admin only). Deactivating a project removes it from the dashboard but
 * does not delete any data.
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

$body         = json_decode(file_get_contents('php://input'), true);
$id           = (int)($body['id']                      ?? 0);
$name         = trim(strip_tags($body['name']          ?? ''));
$description  = trim(strip_tags($body['description']   ?? '')) ?: null;
$year         = (int)($body['year']                    ?? date('Y'));
$centreNumber = trim(strip_tags($body['centre_number'] ?? ''));
$baseHours    = (float)($body['base_hours']            ?? 30);
$startDate    = $body['start_date'] ?? null ?: null;
$endDate      = $body['end_date']   ?? null ?: null;
$isActive     = isset($body['is_active']) ? (int)(bool)$body['is_active'] : 1;

if (!$name) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Project name is required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'UPDATE projects SET name=?,description=?,year=?,centre_number=?,base_hours=?,
         start_date=?,end_date=?,is_active=? WHERE id=?'
    );
    $stmt->execute([$name, $description, $year, $centreNumber, $baseHours, $startDate, $endDate, $isActive, $id]);
    echo json_encode(['success' => true, 'data' => ['message' => 'Project updated']]);
} catch (\Throwable $e) {
    http_response_code(500);
    echo json_encode(['success' => false, 'error' => 'Server error']);
}
