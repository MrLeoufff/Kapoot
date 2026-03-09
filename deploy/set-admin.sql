-- Mettre un utilisateur en admin (à exécuter sur le serveur avec ton mot de passe MySQL)
-- Remplacer 'ton@email.com' par l'email du compte à promouvoir
UPDATE Users SET IsAdmin = 1 WHERE Email = 'ton@email.com';
