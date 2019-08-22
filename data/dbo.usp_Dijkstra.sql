-- Runs Dijkstras algorithm from the specified node.
-- @StartNode: origin -> the node to start from.
-- @EndNode: destination-> Stop the search when the shortest path to this node is found.
-- Specify NULL find shortest path to all nodes.
CREATE PROCEDURE dbo.usp_Dijkstra (@StartNode nchar(10), @EndNode nchar(10) = NULL)
AS
BEGIN
    -- Automatically rollback the transaction if something goes wrong.    
    SET XACT_ABORT ON    
    BEGIN TRAN
    
	-- Increase performance and do not intefere with the results.
    SET NOCOUNT ON;

    -- Create a temporary table for storing the estimates as the algorithm runs
	CREATE TABLE #Nodes
	(
		Id nchar(10) NOT NULL ,    -- The IATA3
		Estimate decimal(10,3) NOT NULL,    -- What is the distance to this node, so far?
		Predecessor nchar(10) NULL,    -- The node we came from to get to this node with this distance.
		Done bit NOT NULL        -- Are we done with this node yet (is the estimate the final distance)?
	)
	-- Fill the temporary table with initial data
    INSERT INTO #Nodes (Id, Estimate, Predecessor, Done)
    SELECT IATA3, 9999999.999, NULL, 0 FROM dbo.airports

	-- Create temporary table for routes with calculated distanse in Miles
	CREATE TABLE #Routes
	(
		ArlineId nchar(10) NOT NULL ,    -- The IATA3
		Origin  nchar(10) NOT NULL,    -- What is the distance to this node, so far?
		Destination nchar(10) NULL,    -- The node we came from to get to this node with this distance.
		DistanceMi float NOT NULL        -- Are we done with this node yet (is the estimate the final distance)?
	)
     INSERT INTO #Routes (ArlineId, Origin, Destination, DistanceMi)
    Select r.[Airline Id], r.Origin, r.Destination, (select dbo.Distance(a.Latitute,a.Longitude,a2.Latitute,a2.Longitude))  from dbo.routes r
    inner join dbo.airports a on a.IATA3 = r.Origin 
    inner join dbo.airports a2 on a2.IATA3 = r.Destination
	
	-- Check if destination exists
	IF NOt EXISTS (SELECT * FROM #Nodes  WHERE id = @EndNode)
	BEGIN
		DROP TABLE #Nodes
			RAISERROR ('50006', 11, 2) 
			ROLLBACK TRAN        
			RETURN 1
	 END
	

    -- Set the estimate for the node we start in to be 0.
    UPDATE #Nodes SET Estimate = 0 WHERE Id = @StartNode
    IF @@rowcount <> 1
    BEGIN
        DROP TABLE #Nodes
        RAISERROR ('50005', 11, 1) 
        ROLLBACK TRAN        
        RETURN 1
    END

    DECLARE @FromNode nchar(10), @CurrentEstimate int

    -- Run the algorithm until we decide that we are finished
    WHILE 1 = 1
    BEGIN
        -- Reset the variable, so we can detect getting no records in the next step.
        SELECT @FromNode = NULL

        -- Select the Id and current estimate for a node not done, with the lowest estimate.
        SELECT TOP 1 @FromNode = Id, @CurrentEstimate = Estimate
        FROM #Nodes WHERE Done = 0 AND Estimate < 9999999.999
        ORDER BY Estimate
        
        -- Stop if we have no more unvisited, reachable nodes.
        IF @FromNode IS NULL OR @FromNode = @EndNode BREAK

        -- We are now done with this node.
        UPDATE #Nodes SET Done = 1 WHERE Id = @FromNode

        -- Update the estimates to all neighbour node of this one (all the nodes
        -- there are edges to from this node). Only update the estimate if the new
        -- proposal (to go via the current node) is better (lower).
        UPDATE #Nodes
		SET Estimate = @CurrentEstimate + e.DistanceMi, Predecessor = @FromNode
        FROM #Nodes n INNER JOIN #Routes e ON n.Id = e.Destination
        WHERE Done = 0 AND e.Origin = @FromNode AND (@CurrentEstimate + e.DistanceMi) < n.Estimate
        
    END;
    
	-- Select the results. We use a recursive common table expression to
	-- get the full path from the start node to the current node.
	WITH BacktraceCTE(Id, Name, Distance, Path, NamePath)
	AS
	(
		-- Anchor/base member of the recursion, this selects the start node.
		SELECT n.Id, node.Name, n.Estimate, CAST(n.Id AS varchar(8000)),
			CAST(node.Name AS varchar(8000))
		FROM #Nodes n JOIN dbo.airports node ON n.Id = node.[IATA3]
		WHERE n.Id = @StartNode
		
		UNION ALL
		
		-- Recursive member, select all the nodes which have the previous
		-- one as their predecessor. Concat the paths together.
		SELECT n.Id, node.Name,  n.Estimate,
			CAST(cte.Path + '->' + n.Id as varchar(8000)),
			CAST(cte.NamePath + ',' + node.Name AS varchar(8000))
		FROM #Nodes n JOIN BacktraceCTE cte ON n.Predecessor = cte.Id
		JOIN dbo.airports node ON n.Id = node.[IATA3]
	)
	SELECT TOP(1) Path FROM BacktraceCTE
	WHERE Id = @EndNode OR @EndNode IS NULL 
	ORDER BY Id								
    
    DROP TABLE #Nodes
    COMMIT TRAN
    RETURN 0
END