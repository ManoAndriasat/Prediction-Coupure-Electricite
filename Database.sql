-- Drop existing views and tables
DROP VIEW IF EXISTS avg_etudiant;
DROP VIEW IF EXISTS etudiant_present;
DROP TABLE IF EXISTS classe;
DROP TABLE IF EXISTS coupure;
DROP TABLE IF EXISTS source;

CREATE TABLE source (
    id_source VARCHAR(10) PRIMARY KEY,
    nb_panneau DOUBLE PRECISION,
    capacite_panneau DOUBLE PRECISION,
    capacite_batterie DOUBLE PRECISION
);

CREATE TABLE coupure (
    dateCoupure date,
    idSource VARCHAR(10) REFERENCES source(id_source),
    heure int,
    minute int
);


-- Create the 'classe' table and insert data
CREATE TABLE classe (
    classe VARCHAR(10),
    id_source VARCHAR(10) REFERENCES source(id_source)
);


-- Create the 'presence' table and insert data
CREATE TABLE presence (
    date_presence DATE,
    classe VARCHAR(10),
    nb_etudiant_matin DOUBLE PRECISION,
    nb_etudiant_midi DOUBLE PRECISION
);


-- Create the 'luminosite' table and insert data
CREATE TABLE luminosite (
    date_jour DATE,
    heure DOUBLE PRECISION,
    value DOUBLE PRECISION
);

-- Create the 'etudiant_present' view
CREATE OR REPLACE VIEW etudiant_present AS
SELECT
    p.date_presence AS date_presence,
    EXTRACT(DOW FROM p.date_presence) AS day_of_week,
    c.id_source,
    SUM(p.nb_etudiant_matin) AS nb_matin,
    SUM(p.nb_etudiant_midi) AS nb_midi
FROM
    classe c
JOIN
    presence p ON c.classe = p.classe
GROUP BY
    p.date_presence, c.id_source;

-- Create the 'avg_etudiant' view
CREATE OR REPLACE VIEW avg_etudiant AS
SELECT
    day_of_week,
    id_source,
    AVG(nb_matin) AS avg_nb_matin,
    AVG(nb_midi) AS avg_nb_midi
FROM
    etudiant_present
GROUP BY
    id_source, day_of_week;


