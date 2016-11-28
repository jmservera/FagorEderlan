with 
inf_curvas as (
    SELECT
      ref,
      TRY_CAST(tiempo as float) AS tiempo,  
      TRY_CAST(Carrera as float) AS carrera,
      TRY_CAST(Diferenciadepresion as float) AS difpres,
      DATEADD(ms,TRY_CAST(tiempo as float), EventProcessedUtcTime) as timestamp_t,
      TRY_CAST(PresionP2/Contrapresion as float) AS presionp2divContrapresion,
      TRY_CAST(Canal5 as float) AS canal5,
      TRY_CAST(Canal6 as float) AS canal6,
      TRY_CAST(Canal7 as float) AS canal7,
      TRY_CAST(Canal8 as float) AS canal8,
      TRY_CAST(PresionP1 as float) AS presionp1,
      SUBSTRING(ref ,0 ,CHARINDEX('_',ref))  AS referencia,
      --DATEADD(s,(TRY_CAST(SUBSTRING(ref ,7 ,2) as float)*3600 + (TRY_CAST(SUBSTRING(ref ,9 ,2) as float)*60)+ (TRY_CAST(SUBSTRING(ref ,11 ,2) as float))), TRY_CAST(TRY_CAST(CONCAT('20',SUBSTRING(ref ,0 ,3),'.',SUBSTRING(ref ,3 ,2),'.',SUBSTRING(ref ,5 ,2)) AS datetime) AS datetime)) as timecurvas
      DATETIMEFROMPARTS(
                   TRY_CAST(CONCAT('20',SUBSTRING(ref ,1 ,2)) as bigint), -- year
                   TRY_CAST(SUBSTRING(ref ,3 ,2) as bigint),              -- month
                   TRY_CAST(SUBSTRING(ref ,5 ,2) as bigint),              -- day
                   TRY_CAST(SUBSTRING(ref ,7 ,2) as bigint),              -- hour
                   TRY_CAST(SUBSTRING(ref ,9 ,2) as bigint),              -- minute
                   TRY_CAST(SUBSTRING(ref ,11 ,2) as bigint),             -- seconds
                   00                                                     -- miliseconds
        ) AS timecurvas
    FROM curvas 
    timestamp by DATETIMEFROMPARTS(
                   TRY_CAST(CONCAT('20',SUBSTRING(ref ,1 ,2)) as bigint), -- year
                   TRY_CAST(SUBSTRING(ref ,3 ,2) as bigint),              -- month
                   TRY_CAST(SUBSTRING(ref ,5 ,2) as bigint),              -- day
                   TRY_CAST(SUBSTRING(ref ,7 ,2) as bigint),              -- hour
                   TRY_CAST(SUBSTRING(ref ,9 ,2) as bigint),              -- minute
                   TRY_CAST(SUBSTRING(ref ,11 ,2) as bigint),             -- seconds
                   00                                                     -- miliseconds
        )
    ),
--inf_curvas AS (
-- SELECT * 
-- FROM curvas0 
-- timestamp by timecurvas
-- ),

inf_medias AS (
  SELECT ref,
      TRY_CAST(fecha AS datetime) AS fecha2,
      TRY_CAST(SUBSTRING(hora ,0 ,3) as float)  AS horas,
      TRY_CAST(SUBSTRING(hora ,3 ,2) as float)  AS minuto,
      TRY_CAST(SUBSTRING(hora ,6 ,2) as float)  AS segundo,
      hora,
      maquina,
      seriesdemediciones,
      numero,
      [limit],
      --DATEADD(s,((TRY_CAST(SUBSTRING(hora ,0 ,3) as float)*3600) + (TRY_CAST(SUBSTRING(hora ,3 ,2) as float)*60) + TRY_CAST(SUBSTRING(hora ,6 ,2) as float)), TRY_CAST(fecha AS datetime)) as timemedias
      DATETIMEFROMPARTS(
                   TRY_CAST(CONCAT('20',SUBSTRING(ref ,1 ,2)) as bigint), -- year
                   TRY_CAST(SUBSTRING(ref ,3 ,2) as bigint),              -- month
                   TRY_CAST(SUBSTRING(ref ,5 ,2) as bigint),              -- day
                   TRY_CAST(SUBSTRING(ref ,7 ,2) as bigint),              -- hour
                   TRY_CAST(SUBSTRING(ref ,9 ,2) as bigint),              -- minute
                   TRY_CAST(SUBSTRING(ref ,11 ,2) as bigint),             -- seconds
                   00                                                     -- miliseconds
        ) AS timemedias
  FROM [avg]
  timestamp by DATETIMEFROMPARTS(
                   TRY_CAST(CONCAT('20',SUBSTRING(ref ,1 ,2)) as bigint), -- year
                   TRY_CAST(SUBSTRING(ref ,3 ,2) as bigint),              -- month
                   TRY_CAST(SUBSTRING(ref ,5 ,2) as bigint),              -- day
                   TRY_CAST(SUBSTRING(ref ,7 ,2) as bigint),              -- hour
                   TRY_CAST(SUBSTRING(ref ,9 ,2) as bigint),              -- minute
                   TRY_CAST(SUBSTRING(ref ,11 ,2) as bigint),             -- seconds
                   00                                                     -- miliseconds
        )
)

--SELECT * INTO PBIederlan FROM inf_medias 
--SELECT * INTO tableOutput FROM inf_curvas

SELECT c.ref ref, c.tiempo, c.carrera, m.maquina
INTO tableOutput
FROM inf_curvas c 
LEFT OUTER JOIN inf_medias m 
ON c.ref=m.ref
AND DATEDIFF(hour,c,m) BETWEEN -1 AND 1

