SELECT
    REF,TRY_CAST([tiempo1Carrera] as float) tiempo, TRY_CAST([Diferenciadepresion] as float) difpres,
    DATEADD(ms,TRY_CAST([tiempo1Carrera] as float), EventProcessedUtcTime) timestamp_t,  *
FROM
    input
--where [tiempo1Carrera]='1.0'