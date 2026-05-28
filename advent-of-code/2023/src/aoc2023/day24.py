### Generic least squares optimization
### import numpy as np
### from scipy.optimize import least_squares
### 
### # 1. Generic Measurement Function (Residuals)
### def generic_residuals(x, known_independent_data, actual_measurements):
###     """
###     x: array of parameters to optimize (e.g., [slope, intercept])
###     known_independent_data: x-values or inputs
###     actual_measurements: observed y-values or data
###     """
###     # Unpack parameters
###     param1, param2 = x
###     
###     # Your mathematical prediction model (e.g., a line: y = m*x + c)
###     predictions = param1 * known_independent_data + param2
###     
###     # Compute and return the residuals (Measurement - Prediction)
###     residuals = actual_measurements - predictions
###     return residuals
### 
### # 2. Setup your data and initial guesses
### x_data = np.array([1, 2, 3, 4, 5])
### y_measured = np.array([2.1, 3.9, 6.1, 7.8, 10.2])
### initial_guess = [1.0, 0.0]
### 
### # 3. Run the optimizer
### result = least_squares(
###     fun=generic_residuals, 
###     x0=initial_guess, 
###     args=(x_data, y_measured)
### )
### 
### print("Optimized Parameters:", result.x)

# /// script
# dependencies = [
#   "numpy",
#   "scipy",
# ]
# ///

# uv run day24.py

import numpy as np
from scipy.optimize import least_squares

def line_distance_residuals(params, input_points, input_directions):
    """
    Computes the shortest distances from the target line to N input lines.
    
    params: 6-element array [x0, y0, z0, vx0, vy0, vz0] defining the target line.
    input_points: (N, 3) array of points on the input lines.
    input_directions: (N, 3) array of direction vectors for the input lines.
    """
    # Extract target line point and direction
    p0 = params[0:3]
    v0 = params[3:6]
    
    # Normalize the target direction vector to ensure it remains a unit vector
    v0_norm = np.linalg.norm(v0)
    if v0_norm < 1e-8:
        v0 = np.array([1.0, 0.0, 0.0])  # Fallback for degenerate state
    else:
        v0 = v0 / v0_norm

    residuals = []
    
    for pi, vi in zip(input_points, input_directions):
        # Ensure input direction is normalized
        vi = vi / np.linalg.norm(vi)
        
        # Cross product of the two direction vectors
        cross_v = np.cross(vi, v0)
        cross_norm = np.linalg.norm(cross_v)
        
        if cross_norm < 1e-8:
            # Lines are parallel; distance is the projection perpendicular to the direction
            diff = pi - p0
            dist = np.linalg.norm(diff - np.dot(diff, v0) * v0)
        else:
            # Lines are skew or intersecting; use the standard shortest distance formula
            dist = np.abs(np.dot(pi - p0, cross_v)) / cross_norm
            
        residuals.append(dist)
        
    return np.array(residuals)

# --- Example Usage ---

# Define 5 input lines
# Line 1
p1 = np.array([19.0, 13.0, 30.0])
v1 = np.array([-2.0, 1.0, -2.0])

# Line 2
p2 = np.array([18.0, 19.0, 22.0])
v2 = np.array([-1.0, -1.0, -2.0])

# Line 3
p3 = np.array([20.0, 25.0, 34.0])
v3 = np.array([-2.0, -2.0, -4.0])

# Line 4
p4 = np.array([12.0, 31.0, 28.0])
v4 = np.array([-1.0, -2.0, -1.0])

# Line 5
p5 = np.array([20.0, 19.0, 15.0])
v5 = np.array([1.0, -5.0, -3.0])

input_pts = np.array([p1, p2, p3, p4, p5])
input_dirs = np.array([v1, v2, v3, v4, v5])

# Initial guess for the target line: starting at (0.5, 0.5, 0.5) moving along [1, 1, 1]
# [x0, y0, z0, vx0, vy0, vz0]
initial_guess = np.array([0.5, 0.5, 0.5, 1.0, 1.0, 1.0])

# Run Levenberg-Marquardt or Trust-Region Reflective via least_squares
result = least_squares(
    fun=line_distance_residuals,
    x0=initial_guess,
    args=(input_pts, input_dirs),
    method='trf'
)

# Extract and clean up results
opt_p0 = result.x[0:3]
opt_v0 = result.x[3:6]
opt_v0 = opt_v0 / np.linalg.norm(opt_v0)

print("Optimization Success:", result.success)
print("Optimized Point on Line (p0):      ", np.round(opt_p0, 4))
print("Optimized Unit Direction Vector (v0):", np.round(opt_v0, 4))
print("Residual distances per line:        ", np.round(result.fun, 4))
print("Total Cost (Sum of squares / 2):     ", round(result.cost, 4))

