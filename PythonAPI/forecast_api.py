import pandas as pd
import numpy as np
from flask import Flask, request, jsonify
from statsmodels.tsa.arima.model import ARIMA
import warnings
from datetime import datetime, timedelta

warnings.filterwarnings("ignore")
app = Flask(__name__)

@app.route('/forecast', methods=['POST'])
def forecast():
    try:
        data = request.json.get('data', [])
        if not data:
            return jsonify({"error": "No data provided"}), 400
        
        df = pd.DataFrame(data)
        df['date'] = pd.to_datetime(df['date'])
        
        daily_sales = df.groupby('date')['amount'].sum().reset_index()
        daily_sales = daily_sales.set_index('date').asfreq('D').fillna(0)
        
        model = ARIMA(daily_sales, order=(1, 1, 1))
        model_fit = model.fit()
        
        forecast = model_fit.forecast(steps=30)
        forecast_dates = [datetime.now() + timedelta(days=i) for i in range(1, 31)]
        
        return jsonify([{
            "date": d.strftime("%Y-%m-%d"),
            "amount": float(a)
        } for d, a in zip(forecast_dates, forecast)])
        
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)