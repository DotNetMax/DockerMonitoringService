SELECT COUNT(*) as value
FROM "Containers"
WHERE "State" = 'running'

--> inactive

SELECT COUNT(*) as value
FROM "Containers"
WHERE "State" = 'stopped'