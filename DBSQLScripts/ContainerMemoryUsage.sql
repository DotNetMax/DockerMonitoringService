SELECT "StatDate" as time
    , "Containers"."Name" as metric
    , "MemoryUsage" as value
FROM "ContainerStats"
JOIN "Containers" ON "Containers"."Id" = "ContainerStats"."ContainerId"