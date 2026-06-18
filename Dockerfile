FROM python:3.10-slim

WORKDIR /app

COPY python/requirements.txt python/
RUN pip install --no-cache-dir -r python/requirements.txt

COPY config/ config/
COPY python/ python/

# Unity Editor must run on the host for ML-Agents training.
# This image provides reproducible Python/trainer dependencies.
CMD ["mlagents-learn", "--help"]
