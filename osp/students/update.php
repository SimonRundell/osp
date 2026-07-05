<?php
/**
 * PUT /students/update.php
 *
 * Updates a student's identifiers and name fields. Available to any
 * authenticated staff member (not admin-gated).
 *
 * @package OSPTracker
 */

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../jwt.php';

requireAuth();

if ($_SERVER['REQUEST_METHOD'] !== 'PUT') {
    http_response_code(405);
    echo json_encode(['success' => false, 'error' => 'Method not allowed']);
    exit();
}

$body            = json_decode(file_get_contents('php://input'), true);
$id              = (int)($body['id']               ?? 0);
$candidateNumber = trim(strip_tags($body['candidate_number'] ?? ''));
$cisRef          = trim(strip_tags($body['cis_ref']          ?? '')) ?: null;
$surname         = trim(strip_tags($body['surname']          ?? ''));
$firstName       = trim(strip_tags($body['first_name']       ?? ''));

if (!$candidateNumber || !$surname || !$firstName) {
    http_response_code(400);
    echo json_encode(['success' => false, 'error' => 'Candidate number, surname and first name are required']);
    exit();
}

try {
    $db   = getDb();
    $stmt = $db->prepare(
        'UPDATE students SET candidate_number=?, cis_ref=?, surname=?, first_name=? WHERE id=?'
    );
    $stmt->execute([$candidateNumber, $cisRef, $surname, $firstName, $id]);
    echo json_encode(['success' => true, 'data' => ['message' => 'Student updated']]);
} catch (\Throwable $e) {
    if ((int)$e->getCode() === 23000 || str_contains($e->getMessage(), '23000')) {
        http_response_code(409);
        echo json_encode(['success' => false, 'error' => 'Candidate number already exists']);
    } else {
        http_response_code(500);
        echo json_encode(['success' => false, 'error' => 'Server error']);
    }
}
