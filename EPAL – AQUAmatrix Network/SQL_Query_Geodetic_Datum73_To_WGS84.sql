USE [EPL_AQM]
GO

/****** Object:  UserDefinedFunction [dbo].[Convert_GeodeticCoord_Datum73_To_WGS84]    Script Date: 05/14/2012 10:45:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







-- ======================================================
-- Author:		Ruben Pereira
-- Create date: 06-03-2012 11:05
-- Description:	<Convert_GeodeticCoord_Datum73_To_WGS84>
-- ======================================================

CREATE FUNCTION [dbo].[Convert_GeodeticCoord_Datum73_To_WGS84]
(
	@Datum73_Lat REAL,	-- Degrees
	@Datum73_Lon REAL,	-- Degrees
	@Datum73_Alt REAL
)
RETURNS @T TABLE (Lat REAL, Lon REAL, Alt REAL)
AS
BEGIN
	
	-- Parameter Declaration
	DECLARE @RadiusM as REAL;
	DECLARE @RadiusN as REAL;
	
	DECLARE @e as REAL;
	DECLARE @a as REAL;
	DECLARE @f as REAL;
	
	DECLARE @Delta_X as REAL;
	DECLARE @Delta_Y as REAL;
	DECLARE @Delta_Z as REAL;
	DECLARE @Delta_a as REAL;
	DECLARE @Delta_f as REAL;
	
	DECLARE @Delta_LAT as REAL;
	DECLARE @Delta_LON as REAL;
	DECLARE @Delta_ALT as REAL;
	
	DECLARE @WGS84_Lat REAL;
	DECLARE @WGS84_Lon REAL;
	DECLARE @WGS84_Alt REAL;
	
	-- Parameter Definition	- Hayford Ellipsoid to WGS84
	SET @a = 6378388.0;	-- [meters]
	SET @f = 1/297.0;
	SET @e = SQRT(1 - POWER((1 - @f), 2));
	
	SET @RadiusM = @a * (1 - POWER(@e, 2)) / POWER((1 - (POWER(@e, 2) * POWER(SIN([dbo].[Degrees_To_Radians](@Datum73_Lat)), 2))),1.5);		-- [meters]
	SET @RadiusN = @a / POWER((1 - (POWER(@e, 2) * POWER(SIN([dbo].[Degrees_To_Radians](@Datum73_Lat)), 2))), 0.5);							-- [meters]
	
	SET @Delta_X = -223.237;	-- [meters]
	SET @Delta_Y = 110.193;		-- [meters]
	SET @Delta_Z = 36.649;		-- [meters]
	SET @Delta_a = -251;
	SET @Delta_f = -1.4192702E-5;
	
	-- [radians]
	SET @Delta_LAT = (- @Delta_X * SIN([dbo].[Degrees_To_Radians](@Datum73_Lat)) * COS([dbo].[Degrees_To_Radians](@Datum73_Lon))
				  - @Delta_Y * SIN([dbo].[Degrees_To_Radians](@Datum73_Lat)) * SIN([dbo].[Degrees_To_Radians](@Datum73_Lon))
				  + @Delta_Z * COS([dbo].[Degrees_To_Radians](@Datum73_Lat))
				  + @Delta_a * (@RadiusN * POWER(@e, 2) * SIN([dbo].[Degrees_To_Radians](@Datum73_Lat)) * COS([dbo].[Degrees_To_Radians](@Datum73_Lat)) / @a)
				  + @Delta_f * SIN([dbo].[Degrees_To_Radians](@Datum73_Lat)) * COS([dbo].[Degrees_To_Radians](@Datum73_Lat)) * ((@RadiusM / (1 - @f)) + @RadiusN * (1 - @f))
				  ) / (@RadiusM + @Datum73_Alt);
	
	-- [radians]
	SET @Delta_LON = (- @Delta_X * SIN([dbo].[Degrees_To_Radians](@Datum73_Lon)) + @Delta_Y * COS([dbo].[Degrees_To_Radians](@Datum73_Lon))
				  ) / ((@RadiusN + @Datum73_Alt) * COS([dbo].[Degrees_To_Radians](@Datum73_Lat)));
	
	-- [meters]
	SET @Delta_ALT = 0.0;	-- To complete only if necessary
	
	SET @WGS84_Lat = @Datum73_Lat + @Delta_LAT * 180 / PI();	-- [degrees]
	SET @WGS84_Lon = @Datum73_Lon + @Delta_LON * 180 / PI();	-- [degrees]
	SET @WGS84_Alt = @Datum73_Alt + @Delta_ALT;					-- [meters]
	
	INSERT INTO @T(Lat, Lon, Alt) VALUES (@WGS84_Lat, @WGS84_Lon, @WGS84_Alt)
	RETURN
END





GO


