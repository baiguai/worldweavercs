SELECT
    ce.*
FROM
    element e,
    element ce
WHERE 1=1
    AND ce.ParentKey = e.ElementKey

    AND e.Name = 'Key'
    AND e.ParentKey = 'put_in_test_room'
;


SELECT
    ce.*
FROM
    element e,
    element ce
WHERE 1=1
    AND ce.ParentKey = e.ElementKey

    AND e.Name = 'ornate box'
    AND e.ParentKey = 'put_in_test_room'
;