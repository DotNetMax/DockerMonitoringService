SELECT "StatDate" as time
    , "Containers"."Name" as metric
    , "NetworkUsage" as value
FROM "ContainerStats"
JOIN "Containers" ON "Containers"."Id" = "ContainerStats"."ContainerId"