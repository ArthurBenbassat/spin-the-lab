FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /opt/build

RUN apt update && apt install -y build-essential && curl -fsSL https://sh.rustup.rs | bash -s -- -y && export PATH=$PATH:$HOME/.cargo/bin && rustup default stable && rustup target add wasm32-unknown-unknown && cargo install wizer
RUN curl -fsSL https://developer.fermyon.com/downloads/install.sh | bash && mv spin /usr/local/bin/
RUN apt update && apt install -y nuget

WORKDIR /opt/build
COPY ./Home.csproj .
RUN dotnet restore
 


WORKDIR /opt/build/
COPY . .
RUN spin build

FROM scratch
COPY --from=build /opt/build/bin/Release/net7.0/Home.wasm .
COPY --from=build /opt/build/spin.toml .