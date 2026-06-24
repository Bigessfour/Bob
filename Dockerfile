FROM python:3.10.12-slim-bookworm

WORKDIR /app

# ML-Agents connects to Unity Editor on the host via gRPC (port 5004).
ENV PYTHONUNBUFFERED=1
ENV PIP_DISABLE_PIP_VERSION_CHECK=1

COPY python/requirements.txt python/
RUN pip install --no-cache-dir --upgrade pip setuptools==65.5.0 wheel \
    && pip install --no-cache-dir "torch>=2.1.1,<=2.8.0" \
    && pip install --no-cache-dir -r python/requirements.txt \
    && python -c "import torch; print('TORCH_VERSION:', torch.__version__)"

COPY config/ config/
COPY python/ python/

EXPOSE 5004

# Unity Editor runs on the host; this image provides mlagents-learn.
CMD ["mlagents-learn", "--help"]
