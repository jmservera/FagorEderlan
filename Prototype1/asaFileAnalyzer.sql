with curvas as (SELECT
    REF,TRY_CAST(tiempo1Carrera as float) tiempo,
    TRY_CAST(Diferenciadepresion as float) difpres,
    DATEADD(ms,TRY_CAST(tiempo1Carrera as float), EventProcessedUtcTime) timestamp_t,
    TRY_CAST(presionp2 as float) presionp2,
    TRY_CAST(contrapresion as float) contrapresion,
    TRY_CAST(canal5 as float) canal5,
    TRY_CAST(canal6 as float) canal6,
    TRY_CAST(canal7 as float) canal7,
    TRY_CAST(canal8 as float) canal8,
    TRY_CAST(presionp1 as float) presionp1
FROM
    input
timestamp by DATEADD(ms,TRY_CAST(tiempo1Carrera as float), EventProcessedUtcTime)
    )
select * from curvas
--where tiempo<2;
