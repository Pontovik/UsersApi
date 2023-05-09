--
-- PostgreSQL database dump
--

-- Dumped from database version 15.2
-- Dumped by pg_dump version 15.2

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_table_access_method = heap;

--
-- Name: user_state; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.user_state (
    id integer NOT NULL,
    code character varying(7) NOT NULL,
    description character varying,
    CONSTRAINT user_state_code_check CHECK (((code)::text = ANY ((ARRAY['Active'::character varying, 'Blocked'::character varying])::text[])))
);


--
-- Name: user_state_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.user_state_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: user_state_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.user_state_id_seq OWNED BY public.user_state.id;


--
-- Name: user_state id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_state ALTER COLUMN id SET DEFAULT nextval('public.user_state_id_seq'::regclass);


--
-- Data for Name: user_state; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.user_state (id, code, description) FROM stdin;
1	Blocked	Пользователь заблокирован
2	Active	Пользователь активен
\.


--
-- Name: user_state_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.user_state_id_seq', 2, true);


--
-- Name: user_state user_state_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_state
    ADD CONSTRAINT user_state_pkey PRIMARY KEY (id);


--
-- PostgreSQL database dump complete
--

