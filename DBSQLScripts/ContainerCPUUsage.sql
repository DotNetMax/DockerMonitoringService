SELECT "StatDate" as time
    , "Containers"."Name" as metric
    , "CPUUsage" as value
FROM "ContainerStats"
JOIN "Containers" ON "Containers"."Id" = "ContainerStats"."ContainerId"