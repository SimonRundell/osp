SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- Staff login accounts
DROP TABLE IF EXISTS `staff`;
CREATE TABLE `staff` (
  `id`                   int NOT NULL AUTO_INCREMENT,
  `username`             varchar(50)  NOT NULL,
  `email`                varchar(100) NULL DEFAULT NULL,
  `password_hash`        varchar(255) NOT NULL,
  `first_name`           varchar(50)  NOT NULL,
  `last_name`            varchar(50)  NOT NULL,
  `role`                 enum('admin','staff') NOT NULL DEFAULT 'staff',
  `must_change_password` tinyint(1) NOT NULL DEFAULT 1,
  `is_active`            tinyint(1) NOT NULL DEFAULT 1,
  `last_login`           datetime NULL DEFAULT NULL,
  `created_at`           datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at`           datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `uq_staff_username` (`username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- OSP projects
DROP TABLE IF EXISTS `projects`;
CREATE TABLE `projects` (
  `id`             int NOT NULL AUTO_INCREMENT,
  `name`           varchar(150) NOT NULL,
  `description`    text NULL,
  `year`           year NOT NULL,
  `centre_number`  varchar(20) NOT NULL DEFAULT '54221',
  `base_hours`     decimal(6,2) NOT NULL DEFAULT 30.00,
  `start_date`     date NULL DEFAULT NULL,
  `end_date`       date NULL DEFAULT NULL,
  `created_by`     int NOT NULL,
  `is_active`      tinyint(1) NOT NULL DEFAULT 1,
  `created_at`     datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at`     datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  CONSTRAINT `fk_project_creator` FOREIGN KEY (`created_by`) REFERENCES `staff` (`id`)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Student master records
DROP TABLE IF EXISTS `students`;
CREATE TABLE `students` (
  `id`               int NOT NULL AUTO_INCREMENT,
  `candidate_number` varchar(30) NOT NULL,
  `cis_ref`          varchar(30) NULL DEFAULT NULL,
  `surname`          varchar(60) NOT NULL,
  `first_name`       varchar(60) NOT NULL,
  `is_active`        tinyint(1) NOT NULL DEFAULT 1,
  `created_at`       datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at`       datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `uq_candidate_number` (`candidate_number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Enrolment junction: students on projects with access arrangements
DROP TABLE IF EXISTS `project_students`;
CREATE TABLE `project_students` (
  `id`                     int NOT NULL AUTO_INCREMENT,
  `project_id`             int NOT NULL,
  `student_id`             int NOT NULL,
  `time_extension_percent` tinyint NOT NULL DEFAULT 0 COMMENT '0, 10, 20 or 25',
  `rest_breaks`            tinyint(1) NOT NULL DEFAULT 0,
  `notes`                  text NULL,
  `created_at`             datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at`             datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `uq_project_student` (`project_id`, `student_id`),
  CONSTRAINT `fk_ps_project` FOREIGN KEY (`project_id`) REFERENCES `projects` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_ps_student` FOREIGN KEY (`student_id`) REFERENCES `students` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Supervised working sessions (class or individual)
DROP TABLE IF EXISTS `sessions`;
CREATE TABLE `sessions` (
  `id`             int NOT NULL AUTO_INCREMENT,
  `project_id`     int NOT NULL,
  `session_number` int NOT NULL,
  `session_date`   date NOT NULL,
  `start_time`     time NOT NULL,
  `end_time`       time NOT NULL,
  `supervisor_id`  int NOT NULL,
  `session_type`   enum('class','individual') NOT NULL DEFAULT 'class',
  `notes`          text NULL,
  `created_at`     datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at`     datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `uq_project_session_number` (`project_id`, `session_number`),
  CONSTRAINT `fk_session_project`    FOREIGN KEY (`project_id`)    REFERENCES `projects` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_session_supervisor` FOREIGN KEY (`supervisor_id`) REFERENCES `staff` (`id`)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Minutes present per student per session
DROP TABLE IF EXISTS `session_attendance`;
CREATE TABLE `session_attendance` (
  `id`                  int NOT NULL AUTO_INCREMENT,
  `session_id`          int NOT NULL,
  `project_student_id`  int NOT NULL,
  `minutes_present`     int NOT NULL DEFAULT 0
    COMMENT 'Actual minutes the student worked in this session',
  `created_at`          datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at`          datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `uq_session_student` (`session_id`, `project_student_id`),
  CONSTRAINT `fk_att_session` FOREIGN KEY (`session_id`)         REFERENCES `sessions`         (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_att_ps`      FOREIGN KEY (`project_student_id`) REFERENCES `project_students` (`id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- View: sessions with calculated available_minutes and supervisor name
DROP VIEW IF EXISTS `session_summary`;
CREATE VIEW `session_summary` AS
  SELECT se.id                                                        AS session_id,
         se.project_id,
         p.name                                                       AS project_name,
         se.session_number,
         se.session_date,
         se.start_time,
         se.end_time,
         (TIME_TO_SEC(TIMEDIFF(se.end_time, se.start_time)) / 60)    AS available_minutes,
         se.session_type,
         se.supervisor_id,
         CONCAT(st.first_name, ' ', st.last_name)                    AS supervisor_name,
         se.notes
  FROM   sessions se
  JOIN   projects p  ON p.id  = se.project_id
  JOIN   staff    st ON st.id = se.supervisor_id;

-- View: running totals (allowed / used / remaining) per student per project
DROP VIEW IF EXISTS `student_project_summary`;
CREATE VIEW `student_project_summary` AS
  SELECT ps.id                                                                    AS project_student_id,
         p.id                                                                     AS project_id,
         p.name                                                                   AS project_name,
         p.centre_number,
         p.year,
         s.id                                                                     AS student_id,
         s.candidate_number,
         s.cis_ref,
         s.surname,
         s.first_name,
         ps.time_extension_percent,
         ps.rest_breaks,
         ps.notes,
         ROUND(p.base_hours * 60 * (1 + ps.time_extension_percent / 100), 0)     AS total_minutes_allowed,
         COALESCE(SUM(sa.minutes_present), 0)                                     AS total_minutes_used,
         ROUND(p.base_hours * 60 * (1 + ps.time_extension_percent / 100), 0)
           - COALESCE(SUM(sa.minutes_present), 0)                                 AS minutes_remaining
  FROM       project_students ps
  JOIN       projects          p  ON p.id  = ps.project_id
  JOIN       students          s  ON s.id  = ps.student_id
  LEFT JOIN  session_attendance sa ON sa.project_student_id = ps.id
  GROUP BY   ps.id, p.id, p.name, p.centre_number, p.year,
             s.id, s.candidate_number, s.cis_ref, s.surname, s.first_name,
             ps.time_extension_percent, ps.rest_breaks, ps.notes, p.base_hours;

SET FOREIGN_KEY_CHECKS = 1;