SELECT "StatDate" as time
  , SUM("MemoryUsage") as value
FROM "ContainerStats"
WHERE "StatDate" = (SELECT MAX("StatDate") FROM "ContainerStats")
GROUP BY "StatDate"

--> for time series -->

SELECT "StatDate" as time
  , SUM("MemoryUsage") as value
FROM "ContainerStats"
GROUP BY "StatDate"
ORDER BY "StatDate"