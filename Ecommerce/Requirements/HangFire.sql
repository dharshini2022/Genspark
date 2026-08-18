--
-- PostgreSQL database dump
--

\restrict wqKmOacYaMaqZxlPAEOzq3qZAk0M2WF6yNfOx3M2ZWX3kRqdcJdYjm9ezyFceRA

-- Dumped from database version 18.3
-- Dumped by pg_dump version 18.3

-- Started on 2026-07-09 11:00:20 IST

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 6 (class 2615 OID 27070)
-- Name: hangfire; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA hangfire;


ALTER SCHEMA hangfire OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 289 (class 1259 OID 27425)
-- Name: aggregatedcounter; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.aggregatedcounter (
    id bigint NOT NULL,
    key text NOT NULL,
    value bigint NOT NULL,
    expireat timestamp with time zone
);


ALTER TABLE hangfire.aggregatedcounter OWNER TO postgres;

--
-- TOC entry 288 (class 1259 OID 27424)
-- Name: aggregatedcounter_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: postgres
--

CREATE SEQUENCE hangfire.aggregatedcounter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE hangfire.aggregatedcounter_id_seq OWNER TO postgres;

--
-- TOC entry 4063 (class 0 OID 0)
-- Dependencies: 288
-- Name: aggregatedcounter_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: postgres
--

ALTER SEQUENCE hangfire.aggregatedcounter_id_seq OWNED BY hangfire.aggregatedcounter.id;


--
-- TOC entry 271 (class 1259 OID 27078)
-- Name: counter; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.counter (
    id bigint NOT NULL,
    key text NOT NULL,
    value bigint NOT NULL,
    expireat timestamp with time zone
);


ALTER TABLE hangfire.counter OWNER TO postgres;

--
-- TOC entry 270 (class 1259 OID 27077)
-- Name: counter_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: postgres
--

CREATE SEQUENCE hangfire.counter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE hangfire.counter_id_seq OWNER TO postgres;

--
-- TOC entry 4064 (class 0 OID 0)
-- Dependencies: 270
-- Name: counter_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: postgres
--

ALTER SEQUENCE hangfire.counter_id_seq OWNED BY hangfire.counter.id;


--
-- TOC entry 273 (class 1259 OID 27089)
-- Name: hash; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.hash (
    id bigint NOT NULL,
    key text NOT NULL,
    field text NOT NULL,
    value text,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.hash OWNER TO postgres;

--
-- TOC entry 272 (class 1259 OID 27088)
-- Name: hash_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: postgres
--

CREATE SEQUENCE hangfire.hash_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE hangfire.hash_id_seq OWNER TO postgres;

--
-- TOC entry 4065 (class 0 OID 0)
-- Dependencies: 272
-- Name: hash_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: postgres
--

ALTER SEQUENCE hangfire.hash_id_seq OWNED BY hangfire.hash.id;


--
-- TOC entry 275 (class 1259 OID 27103)
-- Name: job; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.job (
    id bigint NOT NULL,
    stateid bigint,
    statename text,
    invocationdata jsonb NOT NULL,
    arguments jsonb NOT NULL,
    createdat timestamp with time zone NOT NULL,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.job OWNER TO postgres;

--
-- TOC entry 274 (class 1259 OID 27102)
-- Name: job_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: postgres
--

CREATE SEQUENCE hangfire.job_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE hangfire.job_id_seq OWNER TO postgres;

--
-- TOC entry 4066 (class 0 OID 0)
-- Dependencies: 274
-- Name: job_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: postgres
--

ALTER SEQUENCE hangfire.job_id_seq OWNED BY hangfire.job.id;


--
-- TOC entry 286 (class 1259 OID 27182)
-- Name: jobparameter; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.jobparameter (
    id bigint NOT NULL,
    jobid bigint NOT NULL,
    name text NOT NULL,
    value text,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.jobparameter OWNER TO postgres;

--
-- TOC entry 285 (class 1259 OID 27181)
-- Name: jobparameter_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: postgres
--

CREATE SEQUENCE hangfire.jobparameter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE hangfire.jobparameter_id_seq OWNER TO postgres;

--
-- TOC entry 4067 (class 0 OID 0)
-- Dependencies: 285
-- Name: jobparameter_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: postgres
--

ALTER SEQUENCE hangfire.jobparameter_id_seq OWNED BY hangfire.jobparameter.id;


--
-- TOC entry 279 (class 1259 OID 27136)
-- Name: jobqueue; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.jobqueue (
    id bigint NOT NULL,
    jobid bigint NOT NULL,
    queue text NOT NULL,
    fetchedat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.jobqueue OWNER TO postgres;

--
-- TOC entry 278 (class 1259 OID 27135)
-- Name: jobqueue_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: postgres
--

CREATE SEQUENCE hangfire.jobqueue_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE hangfire.jobqueue_id_seq OWNER TO postgres;

--
-- TOC entry 4068 (class 0 OID 0)
-- Dependencies: 278
-- Name: jobqueue_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: postgres
--

ALTER SEQUENCE hangfire.jobqueue_id_seq OWNED BY hangfire.jobqueue.id;


--
-- TOC entry 281 (class 1259 OID 27147)
-- Name: list; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.list (
    id bigint NOT NULL,
    key text NOT NULL,
    value text,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.list OWNER TO postgres;

--
-- TOC entry 280 (class 1259 OID 27146)
-- Name: list_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: postgres
--

CREATE SEQUENCE hangfire.list_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE hangfire.list_id_seq OWNER TO postgres;

--
-- TOC entry 4069 (class 0 OID 0)
-- Dependencies: 280
-- Name: list_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: postgres
--

ALTER SEQUENCE hangfire.list_id_seq OWNED BY hangfire.list.id;


--
-- TOC entry 287 (class 1259 OID 27199)
-- Name: lock; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.lock (
    resource text NOT NULL,
    updatecount integer DEFAULT 0 NOT NULL,
    acquired timestamp with time zone
);


ALTER TABLE hangfire.lock OWNER TO postgres;

--
-- TOC entry 269 (class 1259 OID 27071)
-- Name: schema; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.schema (
    version integer NOT NULL
);


ALTER TABLE hangfire.schema OWNER TO postgres;

--
-- TOC entry 282 (class 1259 OID 27157)
-- Name: server; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.server (
    id text NOT NULL,
    data jsonb,
    lastheartbeat timestamp with time zone NOT NULL,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.server OWNER TO postgres;

--
-- TOC entry 284 (class 1259 OID 27167)
-- Name: set; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.set (
    id bigint NOT NULL,
    key text NOT NULL,
    score double precision NOT NULL,
    value text NOT NULL,
    expireat timestamp with time zone,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.set OWNER TO postgres;

--
-- TOC entry 283 (class 1259 OID 27166)
-- Name: set_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: postgres
--

CREATE SEQUENCE hangfire.set_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE hangfire.set_id_seq OWNER TO postgres;

--
-- TOC entry 4070 (class 0 OID 0)
-- Dependencies: 283
-- Name: set_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: postgres
--

ALTER SEQUENCE hangfire.set_id_seq OWNED BY hangfire.set.id;


--
-- TOC entry 277 (class 1259 OID 27117)
-- Name: state; Type: TABLE; Schema: hangfire; Owner: postgres
--

CREATE TABLE hangfire.state (
    id bigint NOT NULL,
    jobid bigint NOT NULL,
    name text NOT NULL,
    reason text,
    createdat timestamp with time zone NOT NULL,
    data jsonb,
    updatecount integer DEFAULT 0 NOT NULL
);


ALTER TABLE hangfire.state OWNER TO postgres;

--
-- TOC entry 276 (class 1259 OID 27116)
-- Name: state_id_seq; Type: SEQUENCE; Schema: hangfire; Owner: postgres
--

CREATE SEQUENCE hangfire.state_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE hangfire.state_id_seq OWNER TO postgres;

--
-- TOC entry 4071 (class 0 OID 0)
-- Dependencies: 276
-- Name: state_id_seq; Type: SEQUENCE OWNED BY; Schema: hangfire; Owner: postgres
--

ALTER SEQUENCE hangfire.state_id_seq OWNED BY hangfire.state.id;


--
-- TOC entry 3843 (class 2604 OID 27428)
-- Name: aggregatedcounter id; Type: DEFAULT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.aggregatedcounter ALTER COLUMN id SET DEFAULT nextval('hangfire.aggregatedcounter_id_seq'::regclass);


--
-- TOC entry 3826 (class 2604 OID 27247)
-- Name: counter id; Type: DEFAULT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.counter ALTER COLUMN id SET DEFAULT nextval('hangfire.counter_id_seq'::regclass);


--
-- TOC entry 3827 (class 2604 OID 27257)
-- Name: hash id; Type: DEFAULT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.hash ALTER COLUMN id SET DEFAULT nextval('hangfire.hash_id_seq'::regclass);


--
-- TOC entry 3829 (class 2604 OID 27268)
-- Name: job id; Type: DEFAULT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.job ALTER COLUMN id SET DEFAULT nextval('hangfire.job_id_seq'::regclass);


--
-- TOC entry 3840 (class 2604 OID 27321)
-- Name: jobparameter id; Type: DEFAULT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.jobparameter ALTER COLUMN id SET DEFAULT nextval('hangfire.jobparameter_id_seq'::regclass);


--
-- TOC entry 3833 (class 2604 OID 27346)
-- Name: jobqueue id; Type: DEFAULT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.jobqueue ALTER COLUMN id SET DEFAULT nextval('hangfire.jobqueue_id_seq'::regclass);


--
-- TOC entry 3835 (class 2604 OID 27368)
-- Name: list id; Type: DEFAULT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.list ALTER COLUMN id SET DEFAULT nextval('hangfire.list_id_seq'::regclass);


--
-- TOC entry 3838 (class 2604 OID 27378)
-- Name: set id; Type: DEFAULT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.set ALTER COLUMN id SET DEFAULT nextval('hangfire.set_id_seq'::regclass);


--
-- TOC entry 3831 (class 2604 OID 27296)
-- Name: state id; Type: DEFAULT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.state ALTER COLUMN id SET DEFAULT nextval('hangfire.state_id_seq'::regclass);


--
-- TOC entry 4057 (class 0 OID 27425)
-- Dependencies: 289
-- Data for Name: aggregatedcounter; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--

INSERT INTO hangfire.aggregatedcounter VALUES (1, 'stats:succeeded:2026-06-15', 3, '2026-07-15 23:34:58.083403+05:30');
INSERT INTO hangfire.aggregatedcounter VALUES (95, 'stats:succeeded:2026-07-08', 8, '2026-08-08 13:12:05.09212+05:30');
INSERT INTO hangfire.aggregatedcounter VALUES (2, 'stats:succeeded', 50, NULL);
INSERT INTO hangfire.aggregatedcounter VALUES (114, 'stats:succeeded:2026-07-08-07', 2, '2026-07-09 13:12:06.09212+05:30');
INSERT INTO hangfire.aggregatedcounter VALUES (48, 'stats:succeeded:2026-07-05', 19, '2026-08-05 16:26:54.244648+05:30');
INSERT INTO hangfire.aggregatedcounter VALUES (87, 'stats:succeeded:2026-07-07', 3, '2026-08-07 10:02:44.750883+05:30');
INSERT INTO hangfire.aggregatedcounter VALUES (5, 'stats:succeeded:2026-06-16', 15, '2026-07-16 17:26:37.253826+05:30');
INSERT INTO hangfire.aggregatedcounter VALUES (94, 'stats:succeeded:2026-07-08-03', 2, '2026-07-09 09:18:45.964962+05:30');
INSERT INTO hangfire.aggregatedcounter VALUES (42, 'stats:succeeded:2026-06-19', 2, '2026-07-19 17:31:08.891417+05:30');
INSERT INTO hangfire.aggregatedcounter VALUES (100, 'stats:succeeded:2026-07-08-04', 4, '2026-07-09 10:00:26.014485+05:30');


--
-- TOC entry 4039 (class 0 OID 27078)
-- Dependencies: 271
-- Data for Name: counter; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--



--
-- TOC entry 4041 (class 0 OID 27089)
-- Dependencies: 273
-- Data for Name: hash; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--



--
-- TOC entry 4043 (class 0 OID 27103)
-- Dependencies: 275
-- Data for Name: job; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--

INSERT INTO hangfire.job VALUES (44, 205, 'Succeeded', '{"Type": "Ecommerce.Contracts.Services.IJobExecutor, Ecommerce.Contracts", "Method": "DeliverOrder", "Arguments": "[\"37\"]", "ParameterTypes": "[\"System.Int32\"]"}', '["37"]', '2026-07-08 03:26:35.076125+05:30', '2026-07-09 08:58:39.186604+05:30', 0);
INSERT INTO hangfire.job VALUES (50, 229, 'Succeeded', '{"Type": "Ecommerce.Contracts.Services.IJobExecutor, Ecommerce.Contracts", "Method": "DeliverOrder", "Arguments": "[\"40\"]", "ParameterTypes": "[\"System.Int32\"]"}', '["40"]', '2026-07-08 07:27:19.565305+05:30', '2026-07-09 12:59:24.31714+05:30', 0);
INSERT INTO hangfire.job VALUES (49, 232, 'Succeeded', '{"Type": "Ecommerce.Contracts.Services.IJobExecutor, Ecommerce.Contracts", "Method": "ReleaseStock", "Arguments": "[\"40\"]", "ParameterTypes": "[\"System.Int32\"]"}', '["40"]', '2026-07-08 07:26:51.581939+05:30', '2026-07-09 13:12:06.09212+05:30', 0);
INSERT INTO hangfire.job VALUES (43, 208, 'Succeeded', '{"Type": "Ecommerce.Contracts.Services.IJobExecutor, Ecommerce.Contracts", "Method": "ReleaseStock", "Arguments": "[\"37\"]", "ParameterTypes": "[\"System.Int32\"]"}', '["37"]', '2026-07-08 03:26:09.768326+05:30', '2026-07-09 09:18:45.964962+05:30', 0);
INSERT INTO hangfire.job VALUES (46, 213, 'Succeeded', '{"Type": "Ecommerce.Contracts.Services.IJobExecutor, Ecommerce.Contracts", "Method": "DeliverOrder", "Arguments": "[\"38\"]", "ParameterTypes": "[\"System.Int32\"]"}', '["38"]', '2026-07-08 04:07:10.670674+05:30', '2026-07-09 09:39:10.887669+05:30', 0);
INSERT INTO hangfire.job VALUES (48, 218, 'Succeeded', '{"Type": "Ecommerce.Contracts.Services.IJobExecutor, Ecommerce.Contracts", "Method": "DeliverOrder", "Arguments": "[\"39\"]", "ParameterTypes": "[\"System.Int32\"]"}', '["39"]', '2026-07-08 04:15:43.264103+05:30', '2026-07-09 09:47:55.53939+05:30', 0);
INSERT INTO hangfire.job VALUES (45, 221, 'Succeeded', '{"Type": "Ecommerce.Contracts.Services.IJobExecutor, Ecommerce.Contracts", "Method": "ReleaseStock", "Arguments": "[\"38\"]", "ParameterTypes": "[\"System.Int32\"]"}', '["38"]', '2026-07-08 04:06:45.306168+05:30', '2026-07-09 09:51:55.649095+05:30', 0);
INSERT INTO hangfire.job VALUES (47, 224, 'Succeeded', '{"Type": "Ecommerce.Contracts.Services.IJobExecutor, Ecommerce.Contracts", "Method": "ReleaseStock", "Arguments": "[\"39\"]", "ParameterTypes": "[\"System.Int32\"]"}', '["39"]', '2026-07-08 04:15:18.057911+05:30', '2026-07-09 10:00:26.014485+05:30', 0);


--
-- TOC entry 4054 (class 0 OID 27182)
-- Dependencies: 286
-- Data for Name: jobparameter; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--

INSERT INTO hangfire.jobparameter VALUES (45, 43, 'CurrentCulture', '"en-IN"', 0);
INSERT INTO hangfire.jobparameter VALUES (46, 44, 'CurrentCulture', '"en-IN"', 0);
INSERT INTO hangfire.jobparameter VALUES (47, 45, 'CurrentCulture', '"en-IN"', 0);
INSERT INTO hangfire.jobparameter VALUES (48, 46, 'CurrentCulture', '"en-IN"', 0);
INSERT INTO hangfire.jobparameter VALUES (49, 47, 'CurrentCulture', '"en-IN"', 0);
INSERT INTO hangfire.jobparameter VALUES (50, 48, 'CurrentCulture', '"en-IN"', 0);
INSERT INTO hangfire.jobparameter VALUES (51, 49, 'CurrentCulture', '"en-IN"', 0);
INSERT INTO hangfire.jobparameter VALUES (52, 50, 'CurrentCulture', '"en-IN"', 0);


--
-- TOC entry 4047 (class 0 OID 27136)
-- Dependencies: 279
-- Data for Name: jobqueue; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--



--
-- TOC entry 4049 (class 0 OID 27147)
-- Dependencies: 281
-- Data for Name: list; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--



--
-- TOC entry 4055 (class 0 OID 27199)
-- Dependencies: 287
-- Data for Name: lock; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--



--
-- TOC entry 4037 (class 0 OID 27071)
-- Dependencies: 269
-- Data for Name: schema; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--

INSERT INTO hangfire.schema VALUES (23);


--
-- TOC entry 4050 (class 0 OID 27157)
-- Dependencies: 282
-- Data for Name: server; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--



--
-- TOC entry 4052 (class 0 OID 27167)
-- Dependencies: 284
-- Data for Name: set; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--



--
-- TOC entry 4045 (class 0 OID 27117)
-- Dependencies: 277
-- Data for Name: state; Type: TABLE DATA; Schema: hangfire; Owner: postgres
--

INSERT INTO hangfire.state VALUES (202, 44, 'Scheduled', NULL, '2026-07-08 03:26:35.081105+05:30', '{"EnqueueAt": "1783481315075", "ScheduledAt": "1783481195075"}', 0);
INSERT INTO hangfire.state VALUES (204, 44, 'Processing', NULL, '2026-07-08 03:28:39.097967+05:30', '{"ServerId": "dhc47tyx72-dharshinik:58104:aa947837-a2b4-4db0-99dc-ef8f5bb78567", "WorkerId": "2a05d7f6-ba22-4a3d-ab09-842f3e3fa941", "StartedAt": "1783481319089"}', 0);
INSERT INTO hangfire.state VALUES (206, 43, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-07-08 03:48:45.889797+05:30', '{"Queue": "default", "EnqueuedAt": "1783482525878"}', 0);
INSERT INTO hangfire.state VALUES (208, 43, 'Succeeded', NULL, '2026-07-08 03:48:45.966464+05:30', '{"Latency": "1356144", "SucceededAt": "1783482525949", "PerformanceDuration": "35"}', 0);
INSERT INTO hangfire.state VALUES (210, 46, 'Scheduled', NULL, '2026-07-08 04:07:10.676253+05:30', '{"EnqueueAt": "1783483750670", "ScheduledAt": "1783483630670"}', 0);
INSERT INTO hangfire.state VALUES (212, 46, 'Processing', NULL, '2026-07-08 04:09:10.773151+05:30', '{"ServerId": "dhc47tyx72-dharshinik:66545:29b774fd-c3cf-4777-85f9-299688c8d89a", "WorkerId": "5bde9b1f-5e73-44a2-9aed-e4580eb41662", "StartedAt": "1783483750766"}', 0);
INSERT INTO hangfire.state VALUES (215, 48, 'Scheduled', NULL, '2026-07-08 04:15:43.269056+05:30', '{"EnqueueAt": "1783484263263", "ScheduledAt": "1783484143263"}', 0);
INSERT INTO hangfire.state VALUES (217, 48, 'Processing', NULL, '2026-07-08 04:17:55.445631+05:30', '{"ServerId": "dhc47tyx72-dharshinik:67121:c4d0a86e-f4b9-4207-8ce1-b144dc63dffc", "WorkerId": "ceb5e734-e6bb-4cdb-b673-59bce82ffb19", "StartedAt": "1783484275437"}', 0);
INSERT INTO hangfire.state VALUES (219, 45, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-07-08 04:21:55.587348+05:30', '{"Queue": "default", "EnqueuedAt": "1783484515579"}', 0);
INSERT INTO hangfire.state VALUES (221, 45, 'Succeeded', NULL, '2026-07-08 04:21:55.650614+05:30', '{"Latency": "910297", "SucceededAt": "1783484515639", "PerformanceDuration": "36"}', 0);
INSERT INTO hangfire.state VALUES (223, 47, 'Processing', NULL, '2026-07-08 04:30:25.954324+05:30', '{"ServerId": "dhc47tyx72-dharshinik:67121:c4d0a86e-f4b9-4207-8ce1-b144dc63dffc", "WorkerId": "fc210109-54c6-445f-a7e9-f5b63e28aac6", "StartedAt": "1783485025949"}', 0);
INSERT INTO hangfire.state VALUES (225, 49, 'Scheduled', NULL, '2026-07-08 07:26:51.611198+05:30', '{"EnqueueAt": "1783496511573", "ScheduledAt": "1783495611573"}', 0);
INSERT INTO hangfire.state VALUES (227, 50, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-07-08 07:29:24.187802+05:30', '{"Queue": "default", "EnqueuedAt": "1783495764148"}', 0);
INSERT INTO hangfire.state VALUES (229, 50, 'Succeeded', NULL, '2026-07-08 07:29:24.320578+05:30', '{"Latency": "124644", "SucceededAt": "1783495764304", "PerformanceDuration": "94"}', 0);
INSERT INTO hangfire.state VALUES (231, 49, 'Processing', NULL, '2026-07-08 07:42:06.008076+05:30', '{"ServerId": "dhc47tyx72-dharshinik:85751:fa2088f3-9470-4c5a-8f2c-efa23e6ce83b", "WorkerId": "653f7482-72d5-4a96-91b5-0fee747a93ec", "StartedAt": "1783496526004"}', 0);
INSERT INTO hangfire.state VALUES (201, 43, 'Scheduled', NULL, '2026-07-08 03:26:09.794807+05:30', '{"EnqueueAt": "1783482069760", "ScheduledAt": "1783481169760"}', 0);
INSERT INTO hangfire.state VALUES (203, 44, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-07-08 03:28:39.078243+05:30', '{"Queue": "default", "EnqueuedAt": "1783481319048"}', 0);
INSERT INTO hangfire.state VALUES (205, 44, 'Succeeded', NULL, '2026-07-08 03:28:39.189307+05:30', '{"Latency": "124027", "SucceededAt": "1783481319178", "PerformanceDuration": "74"}', 0);
INSERT INTO hangfire.state VALUES (207, 43, 'Processing', NULL, '2026-07-08 03:48:45.907005+05:30', '{"ServerId": "dhc47tyx72-dharshinik:58104:aa947837-a2b4-4db0-99dc-ef8f5bb78567", "WorkerId": "2a05d7f6-ba22-4a3d-ab09-842f3e3fa941", "StartedAt": "1783482525899"}', 0);
INSERT INTO hangfire.state VALUES (209, 45, 'Scheduled', NULL, '2026-07-08 04:06:45.331474+05:30', '{"EnqueueAt": "1783484505297", "ScheduledAt": "1783483605297"}', 0);
INSERT INTO hangfire.state VALUES (211, 46, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-07-08 04:09:10.753549+05:30', '{"Queue": "default", "EnqueuedAt": "1783483750719"}', 0);
INSERT INTO hangfire.state VALUES (213, 46, 'Succeeded', NULL, '2026-07-08 04:09:10.89061+05:30', '{"Latency": "120108", "SucceededAt": "1783483750879", "PerformanceDuration": "99"}', 0);
INSERT INTO hangfire.state VALUES (214, 47, 'Scheduled', NULL, '2026-07-08 04:15:18.082704+05:30', '{"EnqueueAt": "1783485018049", "ScheduledAt": "1783484118049"}', 0);
INSERT INTO hangfire.state VALUES (216, 48, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-07-08 04:17:55.365902+05:30', '{"Queue": "default", "EnqueuedAt": "1783484275322"}', 0);
INSERT INTO hangfire.state VALUES (218, 48, 'Succeeded', NULL, '2026-07-08 04:17:55.541103+05:30', '{"Latency": "132188", "SucceededAt": "1783484275531", "PerformanceDuration": "78"}', 0);
INSERT INTO hangfire.state VALUES (220, 45, 'Processing', NULL, '2026-07-08 04:21:55.599234+05:30', '{"ServerId": "dhc47tyx72-dharshinik:67121:c4d0a86e-f4b9-4207-8ce1-b144dc63dffc", "WorkerId": "ceb5e734-e6bb-4cdb-b673-59bce82ffb19", "StartedAt": "1783484515594"}', 0);
INSERT INTO hangfire.state VALUES (222, 47, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-07-08 04:30:25.941337+05:30', '{"Queue": "default", "EnqueuedAt": "1783485025929"}', 0);
INSERT INTO hangfire.state VALUES (224, 47, 'Succeeded', NULL, '2026-07-08 04:30:26.016357+05:30', '{"Latency": "907901", "SucceededAt": "1783485026009", "PerformanceDuration": "49"}', 0);
INSERT INTO hangfire.state VALUES (226, 50, 'Scheduled', NULL, '2026-07-08 07:27:19.570365+05:30', '{"EnqueueAt": "1783495759564", "ScheduledAt": "1783495639564"}', 0);
INSERT INTO hangfire.state VALUES (228, 50, 'Processing', NULL, '2026-07-08 07:29:24.205944+05:30', '{"ServerId": "dhc47tyx72-dharshinik:68206:f223248b-d02c-4514-b3ed-dbb7d851759f", "WorkerId": "f2449be7-59da-46d6-98ba-43da9ddef098", "StartedAt": "1783495764197"}', 0);
INSERT INTO hangfire.state VALUES (230, 49, 'Enqueued', 'Triggered by DelayedJobScheduler', '2026-07-08 07:42:05.989559+05:30', '{"Queue": "default", "EnqueuedAt": "1783496525931"}', 0);
INSERT INTO hangfire.state VALUES (232, 49, 'Succeeded', NULL, '2026-07-08 07:42:06.095023+05:30', '{"Latency": "914431", "SucceededAt": "1783496526080", "PerformanceDuration": "66"}', 0);


--
-- TOC entry 4072 (class 0 OID 0)
-- Dependencies: 288
-- Name: aggregatedcounter_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: postgres
--

SELECT pg_catalog.setval('hangfire.aggregatedcounter_id_seq', 117, true);


--
-- TOC entry 4073 (class 0 OID 0)
-- Dependencies: 270
-- Name: counter_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: postgres
--

SELECT pg_catalog.setval('hangfire.counter_id_seq', 150, true);


--
-- TOC entry 4074 (class 0 OID 0)
-- Dependencies: 272
-- Name: hash_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: postgres
--

SELECT pg_catalog.setval('hangfire.hash_id_seq', 1, false);


--
-- TOC entry 4075 (class 0 OID 0)
-- Dependencies: 274
-- Name: job_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: postgres
--

SELECT pg_catalog.setval('hangfire.job_id_seq', 50, true);


--
-- TOC entry 4076 (class 0 OID 0)
-- Dependencies: 285
-- Name: jobparameter_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: postgres
--

SELECT pg_catalog.setval('hangfire.jobparameter_id_seq', 52, true);


--
-- TOC entry 4077 (class 0 OID 0)
-- Dependencies: 278
-- Name: jobqueue_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: postgres
--

SELECT pg_catalog.setval('hangfire.jobqueue_id_seq', 58, true);


--
-- TOC entry 4078 (class 0 OID 0)
-- Dependencies: 280
-- Name: list_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: postgres
--

SELECT pg_catalog.setval('hangfire.list_id_seq', 1, false);


--
-- TOC entry 4079 (class 0 OID 0)
-- Dependencies: 283
-- Name: set_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: postgres
--

SELECT pg_catalog.setval('hangfire.set_id_seq', 66, true);


--
-- TOC entry 4080 (class 0 OID 0)
-- Dependencies: 276
-- Name: state_id_seq; Type: SEQUENCE SET; Schema: hangfire; Owner: postgres
--

SELECT pg_catalog.setval('hangfire.state_id_seq', 232, true);


--
-- TOC entry 3885 (class 2606 OID 27437)
-- Name: aggregatedcounter aggregatedcounter_key_key; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.aggregatedcounter
    ADD CONSTRAINT aggregatedcounter_key_key UNIQUE (key);


--
-- TOC entry 3887 (class 2606 OID 27435)
-- Name: aggregatedcounter aggregatedcounter_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.aggregatedcounter
    ADD CONSTRAINT aggregatedcounter_pkey PRIMARY KEY (id);


--
-- TOC entry 3847 (class 2606 OID 27249)
-- Name: counter counter_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.counter
    ADD CONSTRAINT counter_pkey PRIMARY KEY (id);


--
-- TOC entry 3851 (class 2606 OID 27397)
-- Name: hash hash_key_field_key; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.hash
    ADD CONSTRAINT hash_key_field_key UNIQUE (key, field);


--
-- TOC entry 3853 (class 2606 OID 27259)
-- Name: hash hash_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.hash
    ADD CONSTRAINT hash_pkey PRIMARY KEY (id);


--
-- TOC entry 3859 (class 2606 OID 27270)
-- Name: job job_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.job
    ADD CONSTRAINT job_pkey PRIMARY KEY (id);


--
-- TOC entry 3881 (class 2606 OID 27323)
-- Name: jobparameter jobparameter_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.jobparameter
    ADD CONSTRAINT jobparameter_pkey PRIMARY KEY (id);


--
-- TOC entry 3867 (class 2606 OID 27348)
-- Name: jobqueue jobqueue_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.jobqueue
    ADD CONSTRAINT jobqueue_pkey PRIMARY KEY (id);


--
-- TOC entry 3870 (class 2606 OID 27370)
-- Name: list list_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.list
    ADD CONSTRAINT list_pkey PRIMARY KEY (id);


--
-- TOC entry 3883 (class 2606 OID 27238)
-- Name: lock lock_resource_key; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.lock
    ADD CONSTRAINT lock_resource_key UNIQUE (resource);

ALTER TABLE ONLY hangfire.lock REPLICA IDENTITY USING INDEX lock_resource_key;


--
-- TOC entry 3845 (class 2606 OID 27076)
-- Name: schema schema_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.schema
    ADD CONSTRAINT schema_pkey PRIMARY KEY (version);


--
-- TOC entry 3872 (class 2606 OID 27402)
-- Name: server server_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.server
    ADD CONSTRAINT server_pkey PRIMARY KEY (id);


--
-- TOC entry 3876 (class 2606 OID 27405)
-- Name: set set_key_value_key; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.set
    ADD CONSTRAINT set_key_value_key UNIQUE (key, value);


--
-- TOC entry 3878 (class 2606 OID 27380)
-- Name: set set_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.set
    ADD CONSTRAINT set_pkey PRIMARY KEY (id);


--
-- TOC entry 3862 (class 2606 OID 27298)
-- Name: state state_pkey; Type: CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.state
    ADD CONSTRAINT state_pkey PRIMARY KEY (id);


--
-- TOC entry 3848 (class 1259 OID 27445)
-- Name: ix_hangfire_counter_expireat; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_counter_expireat ON hangfire.counter USING btree (expireat);


--
-- TOC entry 3849 (class 1259 OID 27389)
-- Name: ix_hangfire_counter_key; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_counter_key ON hangfire.counter USING btree (key);


--
-- TOC entry 3854 (class 1259 OID 27454)
-- Name: ix_hangfire_hash_expireat; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_hash_expireat ON hangfire.hash USING btree (expireat);


--
-- TOC entry 3855 (class 1259 OID 27472)
-- Name: ix_hangfire_job_expireat; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_job_expireat ON hangfire.job USING btree (expireat);


--
-- TOC entry 3856 (class 1259 OID 27399)
-- Name: ix_hangfire_job_statename; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_job_statename ON hangfire.job USING btree (statename);


--
-- TOC entry 3857 (class 1259 OID 27563)
-- Name: ix_hangfire_job_statename_is_not_null; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_job_statename_is_not_null ON hangfire.job USING btree (statename) INCLUDE (id) WHERE (statename IS NOT NULL);


--
-- TOC entry 3879 (class 1259 OID 27407)
-- Name: ix_hangfire_jobparameter_jobidandname; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_jobparameter_jobidandname ON hangfire.jobparameter USING btree (jobid, name);


--
-- TOC entry 3863 (class 1259 OID 27562)
-- Name: ix_hangfire_jobqueue_fetchedat_queue_jobid; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_jobqueue_fetchedat_queue_jobid ON hangfire.jobqueue USING btree (fetchedat NULLS FIRST, queue, jobid);


--
-- TOC entry 3864 (class 1259 OID 27358)
-- Name: ix_hangfire_jobqueue_jobidandqueue; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_jobqueue_jobidandqueue ON hangfire.jobqueue USING btree (jobid, queue);


--
-- TOC entry 3865 (class 1259 OID 27481)
-- Name: ix_hangfire_jobqueue_queueandfetchedat; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_jobqueue_queueandfetchedat ON hangfire.jobqueue USING btree (queue, fetchedat);


--
-- TOC entry 3868 (class 1259 OID 27492)
-- Name: ix_hangfire_list_expireat; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_list_expireat ON hangfire.list USING btree (expireat);


--
-- TOC entry 3873 (class 1259 OID 27513)
-- Name: ix_hangfire_set_expireat; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_set_expireat ON hangfire.set USING btree (expireat);


--
-- TOC entry 3874 (class 1259 OID 27423)
-- Name: ix_hangfire_set_key_score; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_set_key_score ON hangfire.set USING btree (key, score);


--
-- TOC entry 3860 (class 1259 OID 27307)
-- Name: ix_hangfire_state_jobid; Type: INDEX; Schema: hangfire; Owner: postgres
--

CREATE INDEX ix_hangfire_state_jobid ON hangfire.state USING btree (jobid);


--
-- TOC entry 3889 (class 2606 OID 27334)
-- Name: jobparameter jobparameter_jobid_fkey; Type: FK CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.jobparameter
    ADD CONSTRAINT jobparameter_jobid_fkey FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 3888 (class 2606 OID 27309)
-- Name: state state_jobid_fkey; Type: FK CONSTRAINT; Schema: hangfire; Owner: postgres
--

ALTER TABLE ONLY hangfire.state
    ADD CONSTRAINT state_jobid_fkey FOREIGN KEY (jobid) REFERENCES hangfire.job(id) ON UPDATE CASCADE ON DELETE CASCADE;


-- Completed on 2026-07-09 11:00:20 IST

--
-- PostgreSQL database dump complete
--

\unrestrict wqKmOacYaMaqZxlPAEOzq3qZAk0M2WF6yNfOx3M2ZWX3kRqdcJdYjm9ezyFceRA

