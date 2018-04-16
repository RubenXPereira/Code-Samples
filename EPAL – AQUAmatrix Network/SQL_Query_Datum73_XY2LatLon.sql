USE [EPL_AQM]
GO

/****** Object:  UserDefinedFunction [dbo].[Datum73_XY2LatLon]    Script Date: 05/14/2012 10:48:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		Ruben Pereira
-- Create date: 05-03-2012 11:13
-- Description:	<Datum73_XY2LatLon>
-- =============================================

CREATE FUNCTION [dbo].[Datum73_XY2LatLon]
(
	@X REAL,
	@Y REAL
)
RETURNS @T TABLE (Lat REAL, Lon REAL)
AS
BEGIN
	-- Declarations
	DECLARE @Lat_Hayford REAL
	DECLARE @Lon_Hayford REAL
	
	DECLARE @FE REAL
	DECLARE @FN REAL
	DECLARE @K_0 REAL
	
	-- Transverse Mercator Projection
	DECLARE @a as REAL;
	DECLARE @f as REAL;
	DECLARE @e as REAL;
	
	DECLARE @n as REAL;
	DECLARE @B as REAL;
	
	DECLARE @PHI_0 as REAL;
	DECLARE @LAMBDA_0 as REAL;
	
	DECLARE @S_0 as REAL;
	
	DECLARE @h1 as REAL;
	DECLARE @h2 as REAL;
	DECLARE @h3 as REAL;
	DECLARE @h4 as REAL;
	
	DECLARE @h1_prime as REAL;
	DECLARE @h2_prime as REAL;
	DECLARE @h3_prime as REAL;
	DECLARE @h4_prime as REAL;
	
	DECLARE @Eta_prime as REAL;
	DECLARE @Csi_prime as REAL;
	
	DECLARE @Eta_0_prime as REAL;
	DECLARE @Eta_1_prime as REAL;
	DECLARE @Eta_2_prime as REAL;
	DECLARE @Eta_3_prime as REAL;
	DECLARE @Eta_4_prime as REAL;
	
	DECLARE @Csi_0_prime as REAL;
	DECLARE @Csi_1_prime as REAL;
	DECLARE @Csi_2_prime as REAL;
	DECLARE @Csi_3_prime as REAL;
	DECLARE @Csi_4_prime as REAL;
	
	DECLARE @Beta_prime as REAL;
	DECLARE @Q_prime as REAL;
	DECLARE @Q_prime_prime as REAL;
	
	DECLARE @Q_prime_prime_OLD as REAL;
	DECLARE @DELTA_Q_prime_prime as REAL;
	
	
	-- Attributions / Initializations
	SET @FE = 180.598;
	SET @FN = -86.99;
	SET @K_0 = 1.0;
	
	SET @a = 6378388.0;	-- [meters]
	SET @f = 1/297.0;
	SET @e = SQRT(1 - POWER((1 - @f), 2));
	
	-- Melriça
	SET @PHI_0 = [dbo].Degrees_To_Radians(39.666666667);
	SET @LAMBDA_0 = [dbo].Degrees_To_Radians(-8.1319061111);
	
	SET @n = @f / (2.0 - @f);
	SET @B = (@a / (1.0 + @n)) * (1.0 + (POWER(@n, 2)) / 4.0 + (POWER(@n, 4)) / 64.0);
	
	IF (@PHI_0 != 0)
	BEGIN
		-- Local Variables
		DECLARE @Q_0 as REAL;
		DECLARE @Beta_0 as REAL;
		DECLARE @Csi_0_0 as REAL;
		
		DECLARE @Csi_0_1 as REAL;
		DECLARE @Csi_0_2 as REAL;
		DECLARE @Csi_0_3 as REAL;
		DECLARE @Csi_0_4 as REAL;
		
		DECLARE @Csi_0 as REAL;
		
		-- Formulas
		SET @Q_0 = [dbo].aSinh(tan(@PHI_0)) - @e * [dbo].aTanh(@e * sin(@PHI_0));
		SET @Beta_0 = atan([dbo].Sinh(@Q_0));
		SET @Csi_0_0 = atan([dbo].Sinh(@Q_0));
		
		SET @h1 = (@n / 2.0) - (2.0 / 3.0) * POWER(@n, 2) + (5.0 / 16.0) * POWER(@n, 3) + (41.0 / 180.0) * POWER(@n, 4);
		SET @h2 = (13.0 / 48.0) * POWER(@n, 2) - (3.0 / 5.0) * POWER(@n, 3) + (557.0 / 1440.0) * POWER(@n, 4);
		SET @h3 = (61.0 / 240.0) * POWER(@n, 3) - (103.0 / 140.0) * POWER(@n, 4);
		SET @h4 = (49561.0 / 161280.0) * POWER(@n, 4);
		
		SET @Csi_0_1 = @h1 * sin(2 * @Csi_0_0);
		SET @Csi_0_2 = @h2 * sin(4 * @Csi_0_0);
		SET @Csi_0_3 = @h3 * sin(6 * @Csi_0_0);
		SET @Csi_0_4 = @h4 * sin(8 * @Csi_0_0);
		
		SET @Csi_0 = @Csi_0_0 + @Csi_0_1 + @Csi_0_2 + @Csi_0_3 + @Csi_0_4;
		SET @S_0 = @B * @Csi_0;
		
	END
	
	ELSE
	BEGIN
		SET @S_0 = 0;
	END
	
	-- (Reverse calculation...)
	SET @h1_prime = (@n / 2.0) - (2.0 / 3.0) * POWER(@n, 2) + (37.0 / 96.0) * POWER(@n, 3) - (1.0 / 360.0) * POWER(@n, 4);
	SET @h2_prime = (1.0 / 48.0) * POWER(@n, 2) + (1.0 / 15.0) * POWER(@n, 3) - (437.0 / 1440.0) * POWER(@n, 4);
	SET @h3_prime = (17.0 / 480.0) * POWER(@n, 3) - (37.0 / 840.0) * POWER(@n, 4);
	SET @h4_prime = (4397.0 / 161280.0) * POWER(@n, 4);
	
	SET @Eta_prime = (@X - @FE) / (@B * @K_0);
	SET @Csi_prime = ((@Y - @FN) + (@K_0 * @S_0)) / (@B * @K_0);
	
	SET @Csi_1_prime = @h1_prime * sin(2 * @Csi_prime) * [dbo].Cosh(2 * @Eta_prime);
	SET @Csi_2_prime = @h1_prime * sin(4 * @Csi_prime) * [dbo].Cosh(4 * @Eta_prime);
	SET @Csi_3_prime = @h1_prime * sin(6 * @Csi_prime) * [dbo].Cosh(6 * @Eta_prime);
	SET @Csi_4_prime = @h1_prime * sin(8 * @Csi_prime) * [dbo].Cosh(8 * @Eta_prime);
	SET @Csi_0_prime = @Csi_prime - (@Csi_1_prime + @Csi_2_prime + @Csi_3_prime + @Csi_4_prime);
	
	SET @Eta_1_prime = @h1_prime * cos(2 * @Csi_prime) * [dbo].Sinh(2 * @Eta_prime);
	SET @Eta_2_prime = @h1_prime * cos(4 * @Csi_prime) * [dbo].Sinh(4 * @Eta_prime);
	SET @Eta_3_prime = @h1_prime * cos(6 * @Csi_prime) * [dbo].Sinh(6 * @Eta_prime);
	SET @Eta_4_prime = @h1_prime * cos(8 * @Csi_prime) * [dbo].Sinh(8 * @Eta_prime);
	SET @Eta_0_prime = @Eta_prime - (@Eta_1_prime + @Eta_2_prime + @Eta_3_prime + @Eta_4_prime);
	
	SET @Beta_prime = asin(sin(@Csi_0_prime) / [dbo].Cosh(@Eta_0_prime));
	SET @Q_prime = [dbo].aSinh(tan(@Beta_prime));
	SET @Q_prime_prime = @Q_prime + (@e * [dbo].aTanh(@e * [dbo].Tanh(@Q_prime)));
	
	SET @DELTA_Q_prime_prime = @Q_prime_prime - @Q_prime;
	
	WHILE (ABS(@DELTA_Q_prime_prime) > 0.0000001)	-- Set a Residual
	BEGIN
		SET @Q_prime_prime_OLD = @Q_prime_prime;
		SET @Q_prime_prime = @Q_prime + (@e * [dbo].aTanh(@e * [dbo].Tanh(@Q_prime_prime)));
		SET @DELTA_Q_prime_prime = @Q_prime_prime - @Q_prime_prime_OLD;
	END
		
	-- Finally, the computation of what we are seeking (IN DEGREES)!
	SET @Lat_Hayford = atan([dbo].Sinh(@Q_prime_prime)) * 180 / PI();
	SET @Lon_Hayford = (@LAMBDA_0 + asin([dbo].Tanh(@Eta_0_prime) / cos(@Beta_prime))) * 180 / PI();
	
	INSERT INTO @T(Lat, Lon) VALUES (@Lat_Hayford, @Lon_Hayford)
	RETURN
END



GO


