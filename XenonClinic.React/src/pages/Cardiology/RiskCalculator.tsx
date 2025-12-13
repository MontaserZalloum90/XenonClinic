import { useState } from 'react';
import {
  CalculatorIcon,
  HeartIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
  InformationCircleIcon,
} from '@heroicons/react/24/outline';
import type { RiskAssessment, CalculateRiskRequest } from '../../types/cardiology';
import { RiskLevel } from '../../types/cardiology';

export const RiskCalculator = () => {
  const [formData, setFormData] = useState<CalculateRiskRequest>({
    age: 50,
    gender: 'male',
    totalCholesterol: 200,
    hdlCholesterol: 50,
    systolicBP: 120,
    isSmoker: false,
    hasDiabetes: false,
    hasHypertension: false,
    familyHistory: false,
  });

  const [result, setResult] = useState<RiskAssessment | null>(null);
  const [showResult, setShowResult] = useState(false);

  const calculateFraminghamRisk = (data: CalculateRiskRequest): RiskAssessment => {
    // Simplified Framingham Risk Score calculation
    // Note: This is a simplified version. Real implementation should use actual Framingham equations
    let points = 0;

    // Age points
    if (data.gender === 'male') {
      if (data.age >= 70) points += 8;
      else if (data.age >= 60) points += 6;
      else if (data.age >= 50) points += 4;
      else if (data.age >= 40) points += 2;
    } else {
      if (data.age >= 70) points += 7;
      else if (data.age >= 60) points += 5;
      else if (data.age >= 50) points += 3;
      else if (data.age >= 40) points += 1;
    }

    // Total Cholesterol points
    if (data.totalCholesterol >= 280) points += 4;
    else if (data.totalCholesterol >= 240) points += 3;
    else if (data.totalCholesterol >= 200) points += 2;
    else if (data.totalCholesterol >= 160) points += 1;

    // HDL Cholesterol points (protective)
    if (data.hdlCholesterol < 35) points += 2;
    else if (data.hdlCholesterol < 45) points += 1;
    else if (data.hdlCholesterol >= 60) points -= 1;

    // Blood Pressure points
    if (data.systolicBP >= 160) points += 3;
    else if (data.systolicBP >= 140) points += 2;
    else if (data.systolicBP >= 130) points += 1;

    // Risk factors
    if (data.isSmoker) points += 3;
    if (data.hasDiabetes) points += 3;
    if (data.hasHypertension) points += 1;
    if (data.familyHistory) points += 2;

    // Calculate 10-year risk percentage
    let tenYearRisk = 0;
    if (points <= 0) tenYearRisk = 2;
    else if (points <= 4) tenYearRisk = 5;
    else if (points <= 8) tenYearRisk = 10;
    else if (points <= 12) tenYearRisk = 20;
    else if (points <= 16) tenYearRisk = 30;
    else tenYearRisk = 40;

    // Determine risk level
    let riskLevel: RiskLevel;
    if (tenYearRisk < 10) riskLevel = RiskLevel.Low;
    else if (tenYearRisk < 20) riskLevel = RiskLevel.Moderate;
    else if (tenYearRisk < 30) riskLevel = RiskLevel.High;
    else riskLevel = RiskLevel.VeryHigh;

    // Generate recommendations
    const recommendations: string[] = [];

    if (data.isSmoker) {
      recommendations.push('Smoking cessation is critical for reducing cardiovascular risk');
    }
    if (data.totalCholesterol > 200) {
      recommendations.push('Consider dietary modifications and possibly statin therapy');
    }
    if (data.hdlCholesterol < 40) {
      recommendations.push('Increase HDL through exercise and dietary changes');
    }
    if (data.systolicBP > 140) {
      recommendations.push('Blood pressure management with lifestyle changes and/or medication');
    }
    if (data.hasDiabetes) {
      recommendations.push('Strict glycemic control to reduce cardiovascular complications');
    }
    if (tenYearRisk >= 10) {
      recommendations.push('Consider aspirin therapy (consult with physician)');
    }
    if (tenYearRisk >= 20) {
      recommendations.push('Aggressive risk factor modification recommended');
      recommendations.push('Consider referral to cardiology for further evaluation');
    }

    // General recommendations
    recommendations.push('Regular physical activity (at least 150 minutes per week)');
    recommendations.push('Mediterranean diet with emphasis on fruits, vegetables, and whole grains');
    recommendations.push('Maintain healthy body weight (BMI 18.5-24.9)');
    recommendations.push('Regular follow-up with primary care physician');

    const ldlCholesterol = data.totalCholesterol - data.hdlCholesterol - 50; // Simplified LDL calculation

    return {
      age: data.age,
      gender: data.gender,
      totalCholesterol: data.totalCholesterol,
      hdlCholesterol: data.hdlCholesterol,
      ldlCholesterol,
      systolicBP: data.systolicBP,
      diastolicBP: 80, // Not included in form but could be
      isSmoker: data.isSmoker,
      hasDiabetes: data.hasDiabetes,
      hasHypertension: data.hasHypertension,
      familyHistory: data.familyHistory,
      tenYearRisk,
      riskLevel,
      riskScore: points,
      recommendations,
      assessmentDate: new Date().toISOString(),
    };
  };

  const handleCalculate = (e: React.FormEvent) => {
    e.preventDefault();
    const assessment = calculateFraminghamRisk(formData);
    setResult(assessment);
    setShowResult(true);
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]:
        type === 'checkbox'
          ? (e.target as HTMLInputElement).checked
          : type === 'number'
          ? Number(value)
          : value,
    }));
  };

  const handleReset = () => {
    setFormData({
      age: 50,
      gender: 'male',
      totalCholesterol: 200,
      hdlCholesterol: 50,
      systolicBP: 120,
      isSmoker: false,
      hasDiabetes: false,
      hasHypertension: false,
      familyHistory: false,
    });
    setResult(null);
    setShowResult(false);
  };

  const getRiskLevelColor = (level: RiskLevel) => {
    switch (level) {
      case RiskLevel.Low:
        return 'bg-green-100 text-green-800 border-green-300';
      case RiskLevel.Moderate:
        return 'bg-yellow-100 text-yellow-800 border-yellow-300';
      case RiskLevel.High:
        return 'bg-orange-100 text-orange-800 border-orange-300';
      case RiskLevel.VeryHigh:
        return 'bg-red-100 text-red-800 border-red-300';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-300';
    }
  };

  const getRiskLevelIcon = (level: RiskLevel) => {
    switch (level) {
      case RiskLevel.Low:
        return <CheckCircleIcon className="h-12 w-12 text-green-600" />;
      case RiskLevel.Moderate:
        return <InformationCircleIcon className="h-12 w-12 text-yellow-600" />;
      case RiskLevel.High:
      case RiskLevel.VeryHigh:
        return <ExclamationTriangleIcon className="h-12 w-12 text-red-600" />;
      default:
        return <HeartIcon className="h-12 w-12 text-gray-600" />;
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Cardiac Risk Calculator</h1>
          <p className="text-gray-600 mt-1">
            Calculate 10-year cardiovascular disease risk using Framingham Risk Score
          </p>
        </div>
        <HeartIcon className="h-10 w-10 text-primary-600" />
      </div>

      {/* Information Banner */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div className="flex">
          <InformationCircleIcon className="h-6 w-6 text-blue-600 flex-shrink-0" />
          <div className="ml-3">
            <h3 className="text-sm font-medium text-blue-800">About This Calculator</h3>
            <p className="text-sm text-blue-700 mt-1">
              This calculator estimates the 10-year risk of developing cardiovascular disease based on
              the Framingham Heart Study. It should be used as a clinical tool and not replace medical
              judgment. Results should be discussed with a healthcare provider.
            </p>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Input Form */}
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center mb-4">
            <CalculatorIcon className="h-6 w-6 text-primary-600 mr-2" />
            <h2 className="text-lg font-semibold text-gray-900">Patient Information</h2>
          </div>

          <form onSubmit={handleCalculate} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Age</label>
                <input
                  type="number"
                  name="age"
                  value={formData.age}
                  onChange={handleChange}
                  min="30"
                  max="100"
                  className="input w-full"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Gender</label>
                <select
                  name="gender"
                  value={formData.gender}
                  onChange={handleChange}
                  className="input w-full"
                  required
                >
                  <option value="male">Male</option>
                  <option value="female">Female</option>
                </select>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Total Cholesterol (mg/dL)
              </label>
              <input
                type="number"
                name="totalCholesterol"
                value={formData.totalCholesterol}
                onChange={handleChange}
                min="100"
                max="400"
                className="input w-full"
                required
              />
              <p className="text-xs text-gray-500 mt-1">Normal: &lt;200 mg/dL</p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                HDL Cholesterol (mg/dL)
              </label>
              <input
                type="number"
                name="hdlCholesterol"
                value={formData.hdlCholesterol}
                onChange={handleChange}
                min="20"
                max="100"
                className="input w-full"
                required
              />
              <p className="text-xs text-gray-500 mt-1">
                Normal: &gt;40 mg/dL (men), &gt;50 mg/dL (women)
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Systolic Blood Pressure (mmHg)
              </label>
              <input
                type="number"
                name="systolicBP"
                value={formData.systolicBP}
                onChange={handleChange}
                min="80"
                max="200"
                className="input w-full"
                required
              />
              <p className="text-xs text-gray-500 mt-1">Normal: &lt;120 mmHg</p>
            </div>

            <div className="space-y-3 pt-2">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  name="isSmoker"
                  checked={formData.isSmoker}
                  onChange={handleChange}
                  className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <span className="ml-2 text-sm text-gray-700">Current Smoker</span>
              </label>

              <label className="flex items-center">
                <input
                  type="checkbox"
                  name="hasDiabetes"
                  checked={formData.hasDiabetes}
                  onChange={handleChange}
                  className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <span className="ml-2 text-sm text-gray-700">Diabetes</span>
              </label>

              <label className="flex items-center">
                <input
                  type="checkbox"
                  name="hasHypertension"
                  checked={formData.hasHypertension}
                  onChange={handleChange}
                  className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <span className="ml-2 text-sm text-gray-700">
                  Hypertension (or on BP medication)
                </span>
              </label>

              <label className="flex items-center">
                <input
                  type="checkbox"
                  name="familyHistory"
                  checked={formData.familyHistory}
                  onChange={handleChange}
                  className="rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <span className="ml-2 text-sm text-gray-700">
                  Family History of Premature CVD
                </span>
              </label>
            </div>

            <div className="flex gap-3 pt-4">
              <button type="submit" className="btn btn-primary flex-1">
                <CalculatorIcon className="h-5 w-5 mr-2" />
                Calculate Risk
              </button>
              <button type="button" onClick={handleReset} className="btn btn-outline">
                Reset
              </button>
            </div>
          </form>
        </div>

        {/* Results Panel */}
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center mb-4">
            <HeartIcon className="h-6 w-6 text-primary-600 mr-2" />
            <h2 className="text-lg font-semibold text-gray-900">Risk Assessment Results</h2>
          </div>

          {showResult && result ? (
            <div className="space-y-6">
              {/* Risk Level Display */}
              <div
                className={`rounded-lg border-2 p-6 text-center ${getRiskLevelColor(
                  result.riskLevel!
                )}`}
              >
                <div className="flex justify-center mb-3">{getRiskLevelIcon(result.riskLevel!)}</div>
                <h3 className="text-2xl font-bold mb-2">
                  {result.riskLevel?.replace('_', ' ').toUpperCase()} RISK
                </h3>
                <p className="text-lg font-semibold">{result.tenYearRisk}%</p>
                <p className="text-sm mt-1">10-Year Cardiovascular Disease Risk</p>
              </div>

              {/* Risk Score */}
              <div className="bg-gray-50 rounded-lg p-4">
                <div className="flex justify-between items-center">
                  <span className="text-sm font-medium text-gray-700">Risk Score</span>
                  <span className="text-lg font-bold text-gray-900">{result.riskScore} points</span>
                </div>
              </div>

              {/* Risk Factors Summary */}
              <div>
                <h4 className="text-sm font-medium text-gray-900 mb-3">Risk Factors Present</h4>
                <div className="space-y-2">
                  {result.totalCholesterol! > 200 && (
                    <div className="flex items-start">
                      <ExclamationTriangleIcon className="h-4 w-4 text-orange-500 mr-2 mt-0.5 flex-shrink-0" />
                      <span className="text-sm text-gray-700">Elevated Total Cholesterol</span>
                    </div>
                  )}
                  {result.hdlCholesterol! < 40 && (
                    <div className="flex items-start">
                      <ExclamationTriangleIcon className="h-4 w-4 text-orange-500 mr-2 mt-0.5 flex-shrink-0" />
                      <span className="text-sm text-gray-700">Low HDL Cholesterol</span>
                    </div>
                  )}
                  {result.systolicBP > 120 && (
                    <div className="flex items-start">
                      <ExclamationTriangleIcon className="h-4 w-4 text-orange-500 mr-2 mt-0.5 flex-shrink-0" />
                      <span className="text-sm text-gray-700">Elevated Blood Pressure</span>
                    </div>
                  )}
                  {result.isSmoker && (
                    <div className="flex items-start">
                      <ExclamationTriangleIcon className="h-4 w-4 text-red-500 mr-2 mt-0.5 flex-shrink-0" />
                      <span className="text-sm text-gray-700">Current Smoker</span>
                    </div>
                  )}
                  {result.hasDiabetes && (
                    <div className="flex items-start">
                      <ExclamationTriangleIcon className="h-4 w-4 text-red-500 mr-2 mt-0.5 flex-shrink-0" />
                      <span className="text-sm text-gray-700">Diabetes</span>
                    </div>
                  )}
                  {result.hasHypertension && (
                    <div className="flex items-start">
                      <ExclamationTriangleIcon className="h-4 w-4 text-orange-500 mr-2 mt-0.5 flex-shrink-0" />
                      <span className="text-sm text-gray-700">Hypertension</span>
                    </div>
                  )}
                  {result.familyHistory && (
                    <div className="flex items-start">
                      <ExclamationTriangleIcon className="h-4 w-4 text-orange-500 mr-2 mt-0.5 flex-shrink-0" />
                      <span className="text-sm text-gray-700">Family History of CVD</span>
                    </div>
                  )}
                </div>
              </div>

              {/* Recommendations */}
              <div>
                <h4 className="text-sm font-medium text-gray-900 mb-3">Recommendations</h4>
                <div className="bg-blue-50 rounded-lg p-4 max-h-64 overflow-y-auto">
                  <ul className="space-y-2">
                    {result.recommendations?.map((rec, index) => (
                      <li key={index} className="flex items-start text-sm text-blue-900">
                        <CheckCircleIcon className="h-4 w-4 text-blue-600 mr-2 mt-0.5 flex-shrink-0" />
                        <span>{rec}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>

              {/* Calculated Lipid Values */}
              <div className="bg-gray-50 rounded-lg p-4">
                <h4 className="text-sm font-medium text-gray-900 mb-3">Lipid Profile</h4>
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="text-gray-600">Total Cholesterol:</span>
                    <p className="font-medium text-gray-900">{result.totalCholesterol} mg/dL</p>
                  </div>
                  <div>
                    <span className="text-gray-600">HDL:</span>
                    <p className="font-medium text-gray-900">{result.hdlCholesterol} mg/dL</p>
                  </div>
                  <div>
                    <span className="text-gray-600">LDL (est.):</span>
                    <p className="font-medium text-gray-900">{result.ldlCholesterol} mg/dL</p>
                  </div>
                  <div>
                    <span className="text-gray-600">TC/HDL Ratio:</span>
                    <p className="font-medium text-gray-900">
                      {(result.totalCholesterol! / result.hdlCholesterol!).toFixed(1)}
                    </p>
                  </div>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex gap-3">
                <button className="btn btn-primary flex-1">Save Assessment</button>
                <button className="btn btn-outline">Print Report</button>
              </div>
            </div>
          ) : (
            <div className="text-center py-12 text-gray-500">
              <HeartIcon className="h-16 w-16 text-gray-300 mx-auto mb-4" />
              <p>Enter patient information and click "Calculate Risk" to see results</p>
            </div>
          )}
        </div>
      </div>

      {/* Reference Information */}
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Reference Information</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <h4 className="text-sm font-medium text-gray-900 mb-2">Risk Categories</h4>
            <div className="space-y-2 text-sm">
              <div className="flex items-center">
                <div className="w-4 h-4 bg-green-500 rounded mr-2"></div>
                <span className="text-gray-700">Low Risk: &lt;10%</span>
              </div>
              <div className="flex items-center">
                <div className="w-4 h-4 bg-yellow-500 rounded mr-2"></div>
                <span className="text-gray-700">Moderate Risk: 10-20%</span>
              </div>
              <div className="flex items-center">
                <div className="w-4 h-4 bg-orange-500 rounded mr-2"></div>
                <span className="text-gray-700">High Risk: 20-30%</span>
              </div>
              <div className="flex items-center">
                <div className="w-4 h-4 bg-red-500 rounded mr-2"></div>
                <span className="text-gray-700">Very High Risk: &gt;30%</span>
              </div>
            </div>
          </div>

          <div>
            <h4 className="text-sm font-medium text-gray-900 mb-2">Normal Ranges</h4>
            <div className="space-y-2 text-sm text-gray-700">
              <p>Total Cholesterol: &lt;200 mg/dL</p>
              <p>LDL Cholesterol: &lt;100 mg/dL</p>
              <p>HDL Cholesterol: &gt;40 mg/dL (men), &gt;50 mg/dL (women)</p>
              <p>Blood Pressure: &lt;120/80 mmHg</p>
            </div>
          </div>
        </div>

        <div className="mt-6 text-xs text-gray-500">
          <p>
            <strong>Disclaimer:</strong> This calculator is a clinical decision support tool and
            should not replace professional medical judgment. The Framingham Risk Score has
            limitations and may not be appropriate for all populations. Always consult with a
            healthcare provider for individualized assessment and treatment recommendations.
          </p>
        </div>
      </div>
    </div>
  );
};
