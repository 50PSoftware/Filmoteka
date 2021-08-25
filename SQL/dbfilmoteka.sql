create database filmy default character set="utf8" default collate = "utf8_czech_ci";
use filmy;
create table film(id int not null auto_increment, nazev varchar(64) not null, filename varchar(128) not null, popis text null, idrok int not null, primary key(id));
create table zanr(id int not null auto_increment, nazev varchar(64) not null, primary key(id));
create table rok(id int not null auto_increment, rok int not null, primary key(id));
create table film_zanr(idfilm int not null, idzanr int not null);

alter table film add constraint fk_film_rok foreign key(idrok) references rok(id);
alter table film_zanr add constraint fk_fz_f foreign key(idfilm) references film(id);
alter table film_zanr add constraint fk_fz_z foreign key(idzanr) references zanr(id);