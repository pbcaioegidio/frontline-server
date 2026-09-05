PB 3.122 BR - database
======================

1. create the database:

   createdb -U postgres pb

2. import everything (schema + all data + all migrations, single file):

   psql -U postgres -d pb -f pb_3122_full.sql

   the migrations/ folder is the individual files, for reference only.
   you do NOT need to run them after importing pb_3122_full.sql.

3. create an account to log in (login is by token, no password check):

   psql -U postgres -d pb -c "INSERT INTO accounts (username, password, token) VALUES ('player1', 'player1', 'AAECAwQFBgcICQoLDA0OMg==');"

   then launch the client with that token:

   PointBlank.exe /token AAECAwQFBgcICQoLDA0OMg== /launcher PBLauncher

   want more accounts? same insert, different username + token, and pass
   that token on the command line instead.

4. point the server at the db: binary/Config/Settings.ini [Database]
   (Host, Port, Name, User, Pass). default is postgres/postgres on
   localhost:5433, db name pb.

notes
- the accounts in the dump are synthetic test rows, ignore or delete them.
- gm access: UPDATE accounts SET access_level = 6 WHERE username = 'player1';
