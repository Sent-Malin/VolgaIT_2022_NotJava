create table users
(
	id serial not null unique,
	email varchar(50) not null,
	password varchar(40) not null
);

create table applications
(
	id serial not null unique,
	name varchar(50) not null,
	date_create date not null,
	user_id int references users (id) on delete cascade not null
);

create table user_requests
(
	id serial not null unique,
	application_id int references applications (id) on delete cascade not null,
	name varchar(50) not null,
	date_request date not null,
	extra_data varchar(255)
);